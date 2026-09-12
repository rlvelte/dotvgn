using DotVgn.Client.Mapper;
using DotVgn.Common.Contracts;
using DotVgn.Common.Enumerations;

namespace DotVgn.Tests.Client;

public sealed class DepartureMapperTests {
    private static DepartureResponseContract.DepartureContract CreateValidContract() => new() {
        Line = "U1",
        StopPoint = "Platform 1",
        Direction = "Richtung1",
        DirectionDescription = "Fürth Hauptbahnhof",
        Date = new DateTimeOffset(2024, 1, 15, 0, 0, 0, TimeSpan.Zero),
        DepartureTimePlanned = new DateTimeOffset(2024, 1, 15, 14, 30, 0, TimeSpan.Zero),
        DepartureTimeActual = new DateTimeOffset(2024, 1, 15, 14, 32, 0, TimeSpan.Zero),
        Transport = "Bus",
        TripNumber = 12345,
        OccupationLevel = "low"
    };

    [Fact]
    public void Map_Single_ValidContract_MapsAllFields() {
        var mapper = new DepartureMapper();
        var contract = CreateValidContract();

        var result = mapper.Map(contract);

        Assert.Equal("U1", result.Line);
        Assert.Equal("Platform 1", result.StopPoint);
        Assert.Equal("Richtung1", result.Direction);
        Assert.Equal("Fürth Hauptbahnhof", result.DirectionDescription);
        Assert.Equal(TransportType.Bus, result.TransportType);
        Assert.Equal(12345, result.TripNumber);
        Assert.Equal("low", result.OccupationLevel);
        Assert.Equal(contract.DepartureTimePlanned.LocalDateTime.Hour, result.DepartureTimePlanned.Hour);
        Assert.Equal(contract.DepartureTimePlanned.LocalDateTime.Minute, result.DepartureTimePlanned.Minute);
    }

    [Fact]
    public void Map_Single_TransportBus_ParsesCorrectly() {
        var mapper = new DepartureMapper();
        var contract = CreateValidContract() with { Transport = "Bus" };

        var result = mapper.Map(contract);

        Assert.Equal(TransportType.Bus, result.TransportType);
    }

    [Fact]
    public void Map_Single_TransportTram_ParsesCorrectly() {
        var mapper = new DepartureMapper();
        var contract = CreateValidContract() with { Transport = "Tram" };

        var result = mapper.Map(contract);

        Assert.Equal(TransportType.Tram, result.TransportType);
    }

    [Fact]
    public void Map_Single_TransportUBahn_ParsesCorrectly() {
        var mapper = new DepartureMapper();
        var contract = CreateValidContract() with { Transport = "UBahn" };

        var result = mapper.Map(contract);

        Assert.Equal(TransportType.UBahn, result.TransportType);
    }

    [Fact]
    public void Map_Single_TransportUnknown_FallsBackToUnknown() {
        var mapper = new DepartureMapper();
        var contract = CreateValidContract() with { Transport = "UnknownType" };

        var result = mapper.Map(contract);

        Assert.Equal(TransportType.Unknown, result.TransportType);
    }

    [Fact]
    public void Map_Single_TransportEmpty_FallsBackToUnknown() {
        var mapper = new DepartureMapper();
        var contract = CreateValidContract() with { Transport = "" };

        var result = mapper.Map(contract);

        Assert.Equal(TransportType.Unknown, result.TransportType);
    }

    [Fact]
    public void Map_Single_TransportWhitespace_FallsBackToUnknown() {
        var mapper = new DepartureMapper();
        var contract = CreateValidContract() with { Transport = "  " };

        var result = mapper.Map(contract);

        Assert.Equal(TransportType.Unknown, result.TransportType);
    }

    [Fact]
    public void Map_Collection_Empty_ReturnsEmptyList() {
        var mapper = new DepartureMapper();

        var result = mapper.Map(Array.Empty<DepartureResponseContract.DepartureContract>());

        Assert.Empty(result);
    }

    [Fact]
    public void Map_Collection_MultipleContracts_MapsAll() {
        var mapper = new DepartureMapper();
        var contracts = new[] {
            CreateValidContract() with { Line = "U1", Transport = "UBahn" },
            CreateValidContract() with { Line = "U2", Transport = "UBahn" },
            CreateValidContract() with { Line = "Bus 35", Transport = "Bus" }
        };

        var result = mapper.Map(contracts);

        Assert.Equal(3, result.Count);
        Assert.Equal("U1", result[0].Line);
        Assert.Equal("U2", result[1].Line);
        Assert.Equal("Bus 35", result[2].Line);
    }

    [Fact]
    public void Map_Single_DateTimeOffset_ConvertsToDateTime() {
        var mapper = new DepartureMapper();
        var contract = CreateValidContract() with {
            DepartureTimeActual = new DateTimeOffset(2024, 6, 15, 8, 45, 0, TimeSpan.FromHours(2)),
            DepartureTimePlanned = new DateTimeOffset(2024, 6, 15, 8, 40, 0, TimeSpan.FromHours(2))
        };

        var result = mapper.Map(contract);

        Assert.Equal(contract.DepartureTimePlanned.LocalDateTime.Hour, result.DepartureTimePlanned.Hour);
        Assert.Equal(contract.DepartureTimePlanned.LocalDateTime.Minute, result.DepartureTimePlanned.Minute);
        Assert.Equal(contract.DepartureTimeActual.LocalDateTime.Hour, result.DepartureTimeActual.Hour);
        Assert.Equal(contract.DepartureTimeActual.LocalDateTime.Minute, result.DepartureTimeActual.Minute);
    }

    [Fact]
    public void Map_Single_TripNumberDefaultZero() {
        var mapper = new DepartureMapper();
        var contract = CreateValidContract() with { TripNumber = 0 };

        var result = mapper.Map(contract);

        Assert.Equal(0, result.TripNumber);
    }
}
