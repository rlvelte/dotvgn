using System.CommandLine;
using DotVgn.Client;
using DotVgn.Client.Queries;
using DotVgn.Common.Enumerations;
using DotVgn.Common.Exceptions;
using DotVgn.Common.Models;
using Spectre.Console;
using System.Text.Json;
using System.Text.Json.Serialization;

return await MonitorCli.BuildRootCommand().Parse(args).InvokeAsync();

internal static class MonitorCli {
    private static readonly IAnsiConsole Stderr = AnsiConsole.Create(new AnsiConsoleSettings {
        Out = new AnsiConsoleOutput(Console.Error),
    });

    private static readonly Argument<string?> StationArgument = new("station") {
        Description = "Station name, or a numeric station id (e.g. 704)",
        Arity = ArgumentArity.ZeroOrOne,
    };

    private static readonly Option<TransportType[]?> TransportsOption = new("--transports", "-t") {
        Description = "Transport filter: u, s, tram, bus, r (comma-separated or repeated)",
        AllowMultipleArgumentsPerToken = true,
        CustomParser = result => {
            var values = result.Tokens
                .SelectMany(t => t.Value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                .ToArray();

            if (values.Length == 0) {
                result.AddError("--transports requires at least one transport type: u, s, tram, bus, r");
                return null;
            }

            var transports = new List<TransportType>();
            foreach (var value in values) {
                var transport = value.ToLowerInvariant() switch {
                    "u" or "ubahn" => TransportType.UBahn,
                    "s" or "sbahn" => TransportType.SBahn,
                    "tram" => TransportType.Tram,
                    "bus" => TransportType.Bus,
                    "r" or "rbahn" => TransportType.RBahn,
                    _ => TransportType.Unknown,
                };

                if (transport == TransportType.Unknown) {
                    result.AddError($"Unknown transport type '{value}'. Valid values: u, s, tram, bus, r");
                    return null;
                }

                if (!transports.Contains(transport)) {
                    transports.Add(transport);
                }
            }

            return [.. transports];
        },
    };

    private static readonly Option<string?> LineOption = new("--line", "-l") {
        Description = "Filter departures by line (e.g. U1); cannot be combined with --transports",
    };

    private static readonly Option<int> LimitOption = new("--limit", "-n") {
        Description = "Maximum number of departures to show (1-2000)",
        DefaultValueFactory = _ => 20,
    };

    private static readonly Option<int> TimespanOption = new("--timespan") {
        Description = "Time window for departures, in minutes (0-1440)",
        DefaultValueFactory = _ => 30,
    };

    private static readonly Option<int> DelayOption = new("--delay", "-d") {
        Description = "Show departures starting this many minutes from now",
        DefaultValueFactory = _ => 0,
    };

    private static readonly Option<bool> JsonOption = new("--json", "-j") {
        Description = "Output departures as JSON instead of a table",
    };

    internal static RootCommand BuildRootCommand() {
        var rootCommand = new RootCommand("Nuremberg transit departure monitor") {
            StationArgument,
            TransportsOption,
            LineOption,
            LimitOption,
            TimespanOption,
            DelayOption,
            JsonOption,
        };

        LineOption.Validators.Add(result => {
            var line = result.GetValue(LineOption);
            if (line != null && result.GetValue(TransportsOption) is { Length: > 0 }) {
                result.AddError("--line and --transports are mutually exclusive.");
            }
            if (line is { Length: 0 }) {
                result.AddError("--line must not be empty.");
            }
        });

        LimitOption.Validators.Add(result => {
            var limit = result.GetValue(LimitOption);
            if (limit < 1) {
                result.AddError("--limit must be at least 1.");
            } else if (limit > 2000) {
                result.AddError("--limit must be at most 2000.");
            }
        });

        TimespanOption.Validators.Add(result => {
            var timespan = result.GetValue(TimespanOption);
            if (timespan < 0) {
                result.AddError("--timespan must not be negative.");
            } else if (timespan > 1440) {
                result.AddError("--timespan must be at most 1440 (24 hours).");
            }
        });

        DelayOption.Validators.Add(result => {
            if (result.GetValue(DelayOption) < 0) {
                result.AddError("--delay must not be negative.");
            }
        });

        rootCommand.Validators.Add(result => {
            var station = result.GetValue(StationArgument);
            if (string.IsNullOrWhiteSpace(station)) {
                result.AddError("A station is required: pass a station name or id.");
            }
        });

        rootCommand.SetAction(async (parseResult, cancellationToken) => {
            var station = parseResult.GetValue(StationArgument);
            var transports = parseResult.GetValue(TransportsOption);
            var line = parseResult.GetValue(LineOption);
            var limit = parseResult.GetValue(LimitOption);
            var timespan = parseResult.GetValue(TimespanOption);
            var delay = parseResult.GetValue(DelayOption);
            var json = parseResult.GetValue(JsonOption);

            return await RunAsync(station, transports, line, limit, timespan, delay, json, cancellationToken);
        });

        return rootCommand;
    }

    private static async Task<int> RunAsync(string? station, TransportType[]? transports, string? line, int limit, int timespan, int delay, bool json, CancellationToken ct) {
        var client = new VgnClient();

        var selected = await ResolveStationAsync(client, station, json, ct);
        if (selected == null) {
            return 1;
        }

        var query = BuildQuery(selected.StationId, transports, line, limit, timespan, delay);
        IReadOnlyList<Departure> departures;
        string stationName;

        try {
            (stationName, departures) = await client.GetDeparturesWithStationAsync(query, ct);
        } catch (DotVgnApiException ex) {
            if (json) {
                WriteJsonError("api-error", ex.Message);
            } else {
                AnsiConsole.MarkupLine($"[{Gruvbox.Red}]API request failed:[/] [{Gruvbox.Gray}]{Markup.Escape(ex.Message)}[/]");
            }
            return 1;
        }

        if (stationName.Length > 0) {
            selected = selected with { Name = stationName };
        }

        if (departures.Count == 0) {
            if (json) {
                Console.WriteLine(JsonOutput(selected, []));
            } else {
                AnsiConsole.MarkupLine($"[{Gruvbox.Red}]No departures found[/]");
            }
            return 1;
        }

        if (json) {
            Console.WriteLine(JsonOutput(selected, departures));
        } else {
            AnsiConsole.Write(RenderDepartures(selected.Name, departures));
        }
        return 0;
    }

    private static async Task<Station?> ResolveStationAsync(VgnClient client, string? station, bool json, CancellationToken cancellationToken) {
        if (int.TryParse(station, out var numericId)) {
            return new Station($"Station {numericId}", numericId, 0, 0, TransportTypes.All);
        }

        var stations = await client.GetStationsAsync(new StationQuery(station!), cancellationToken);

        if (stations.Count == 0) {
            if (json) {
                WriteJsonError("station-not-found", $"No stations found for \"{station}\"");
            } else {
                AnsiConsole.MarkupLine($"[{Gruvbox.Red}]No stations found for \"{Markup.Escape(station!)}\"[/]");
            }
            return null;
        }

        return SelectStation(stations, json);
    }

    private static Station? SelectStation(IReadOnlyList<Station> stations, bool json) {
        if (stations.Count == 1) {
            return stations[0];
        }

        if (!AnsiConsole.Profile.Capabilities.Interactive) {
            var matches = stations
                .Select(s => new StationMatchDto(s.StationId, s.Name))
                .ToArray();

            if (json) {
                WriteJsonError("ambiguous-station", "Multiple stations match and no terminal is available for selection. Use --station-id <id>.", matches);
            } else {
                AnsiConsole.MarkupLine($"[{Gruvbox.Red}]Multiple stations match \"{Markup.Escape(stations[0].Name)}\" and no terminal is available for selection.[/]");
                AnsiConsole.MarkupLine($"[{Gruvbox.Gray}]Matches:[/]");
                foreach (var station in stations) {
                    AnsiConsole.MarkupLine($"  [{Gruvbox.Fg}]{Markup.Escape(station.Name)}[/] [{Gruvbox.Gray}](id: {station.StationId})[/]");
                }
                AnsiConsole.MarkupLine($"[{Gruvbox.Gray}]Use --station-id <id> to select one directly.[/]");
            }
            return null;
        }

        return AnsiConsole.Prompt(
            new SelectionPrompt<Station>()
                .Title($"[{Gruvbox.Yellow}]Multiple stations found. Please select:[/]")
                .PageSize(10)
                .UseConverter(s => $"[{Gruvbox.Fg}]{Markup.Escape(s.Name)}[/] [{Gruvbox.Gray}]({s.StationId})[/]")
                .AddChoices(stations));
    }

    private static void WriteJsonError(string code, string message, StationMatchDto[]? matches = null) {
        var payload = new ErrorPayload(new ErrorDto(code, message, matches));
        Console.WriteLine(JsonSerializer.Serialize(payload, VgnJsonContext.Default.ErrorPayload));
    }

    private static string JsonOutput(Station station, IReadOnlyList<Departure> departures) {
        var payload = new JsonPayload(
            new StationDto(station.StationId, station.Name),
            departures
                .OrderBy(d => d.DepartureTimeActual)
                .Select(d => new DepartureDto(
                    d.Line,
                    d.DirectionDescription,
                    d.TransportType.ToString(),
                    d.DepartureTimePlanned,
                    d.DepartureTimeActual,
                    (int)(d.DepartureTimeActual - d.DepartureTimePlanned).TotalMinutes,
                    d.StopPoint)));

        return JsonSerializer.Serialize(payload, VgnJsonContext.Default.JsonPayload);
    }

    private static DepartureQuery BuildQuery(int stationId, TransportType[]? transports, string? line, int limit, int timespan, int delay) {
        if (line != null) {
            return new DepartureQuery(stationId, line, limit, timespan, delay);
        }

        if (transports is { Length: > 0 }) {
            return new DepartureQuery(stationId, transports, limit, timespan, delay);
        }

        return new DepartureQuery(stationId, TransportTypes.All, limit, timespan, delay);
    }

    private static Table RenderDepartures(string stationName, IReadOnlyList<Departure> departures) {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(new Color(0x92, 0x83, 0x74))
            .Title($"[{Gruvbox.Yellow}]Departures at {Markup.Escape(stationName)}[/]");

        table.AddColumn(new TableColumn($"[{Gruvbox.Fg}]Line[/]").Centered());
        table.AddColumn(new TableColumn($"[{Gruvbox.Fg}]Direction[/]"));
        table.AddColumn(new TableColumn($"[{Gruvbox.Fg}]Time[/]").Centered());
        table.AddColumn(new TableColumn($"[{Gruvbox.Fg}]Delay[/]").Centered());
        table.AddColumn(new TableColumn($"[{Gruvbox.Fg}]Platform[/]").Centered());

        var grouped = departures
            .GroupBy(d => d.TransportType)
            .OrderBy(g => g.Key);

        var isFirst = true;
        foreach (var group in grouped) {
            var (transportName, color) = group.Key switch {
                TransportType.UBahn => ("U-Bahn", Gruvbox.Blue),
                TransportType.SBahn => ("S-Bahn", Gruvbox.Green),
                TransportType.Tram => ("Tram", Gruvbox.Red),
                TransportType.Bus => ("Bus", Gruvbox.Orange),
                TransportType.RBahn => ("R-Bahn", Gruvbox.Aqua),
                _ => ("Other", Gruvbox.Fg),
            };

            if (!isFirst) {
                table.AddEmptyRow();
            }
            table.AddRow(new Markup($"[bold {color}]{transportName}[/]"), new Markup(""), new Markup(""), new Markup(""), new Markup(""));
            isFirst = false;

            foreach (var departure in group.OrderBy(d => d.DepartureTimeActual)) {
                var delayMinutes = (int)(departure.DepartureTimeActual - departure.DepartureTimePlanned).TotalMinutes;
                var delayText = delayMinutes <= 0
                    ? $"[{Gruvbox.Green}]On time[/]"
                    : $"[{Gruvbox.Red}]+{delayMinutes} min[/]";

                var timeText = departure.DepartureTimeActual.ToString("HH:mm");

                table.AddRow(
                    $"[{color}]{Markup.Escape(departure.Line)}[/]",
                    $"[{Gruvbox.Fg}]{Markup.Escape(departure.DirectionDescription)}[/]",
                    $"[{Gruvbox.Fg}]{timeText}[/]",
                    delayText,
                    $"[{Gruvbox.Fg}]{Markup.Escape(departure.StopPoint)}[/]"
                );
            }
        }

        return table;
    }
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, WriteIndented = true)]
[JsonSerializable(typeof(JsonPayload))]
[JsonSerializable(typeof(ErrorPayload))]
internal sealed partial class VgnJsonContext : JsonSerializerContext;

internal sealed record JsonPayload(StationDto Station, IEnumerable<DepartureDto> Departures);

internal sealed record StationDto(int Id, string Name);

internal sealed record DepartureDto(string Line, string Direction, string TransportType, DateTimeOffset Planned, DateTimeOffset Actual, int DelayMinutes, string Platform);

internal sealed record ErrorPayload(ErrorDto Error);

internal sealed record ErrorDto(string Code, string Message, StationMatchDto[]? Matches = null);

internal sealed record StationMatchDto(int Id, string Name);

internal static class TransportTypes {
    public static readonly TransportType[] All = [
        TransportType.UBahn,
        TransportType.SBahn,
        TransportType.Tram,
        TransportType.Bus,
        TransportType.RBahn,
    ];
}

internal static class Gruvbox {
    public const string Fg = "#ebdbb2";
    public const string Red = "#fb4934";
    public const string Green = "#b8bb26";
    public const string Yellow = "#fabd2f";
    public const string Blue = "#83a598";
    public const string Aqua = "#8ec07c";
    public const string Orange = "#fe8019";
    public const string Gray = "#928374";
}
