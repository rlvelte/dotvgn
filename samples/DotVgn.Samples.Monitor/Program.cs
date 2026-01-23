using DotVgn.Client;
using DotVgn.Queries;
using DotVgn.Data.Models;
using DotVgn.Data.Enumerations;
using Spectre.Console;

if (args.Length == 0) {
    AnsiConsole.MarkupLine($"[{Gruvbox.Red}]Usage:[/] dotnet run -- \"<station name>\"");
    AnsiConsole.MarkupLine($"[{Gruvbox.Gray}]Example:[/] dotnet run -- \"Hauptbahnhof\"");
    return 1;
}

var stationName = args[0];
var client = new VgnClient();

var stations = await client.GetStationsAsync(new StationQuery(stationName));

if (stations.Count == 0) {
    AnsiConsole.MarkupLine($"[{Gruvbox.Red}]No stations found for \"{Markup.Escape(stationName)}\"[/]");
    return 1;
}

Station selectedStation;
if (stations.Count == 1) {
    selectedStation = stations[0];
} else {
    selectedStation = AnsiConsole.Prompt(
        new SelectionPrompt<Station>()
            .Title($"[{Gruvbox.Yellow}]Multiple stations found. Please select:[/]")
            .PageSize(10)
            .UseConverter(s => $"[{Gruvbox.Fg}]{Markup.Escape(s.Name)}[/] [{Gruvbox.Gray}]({s.StationId})[/]")
            .AddChoices(stations));
}

AnsiConsole.MarkupLine($"[{Gruvbox.Fg}]Fetching departures for[/] [{Gruvbox.Yellow}]{Markup.Escape(selectedStation.Name)}[/][{Gruvbox.Gray}]...[/]");

var allTransports = new[] { TransportType.UBahn, TransportType.SBahn, TransportType.Tram, TransportType.Bus, TransportType.RBahn };
var departures = await client.GetDeparturesAsync(new DepartureQuery(selectedStation.StationId, allTransports, limit: 20, timespan: 30, delay: 0));

if (departures.Count == 0) {
    AnsiConsole.MarkupLine($"[{Gruvbox.Red}]No departures found[/]");
    return 1;
}

var table = new Table()
    .Border(TableBorder.Rounded)
    .BorderColor(new Color(0x92, 0x83, 0x74))
    .Title($"[{Gruvbox.Yellow}]Departures at {Markup.Escape(selectedStation.Name)}[/]");

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
        _ => ("Other", Gruvbox.Fg)
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

AnsiConsole.Write(table);
return 0;

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
