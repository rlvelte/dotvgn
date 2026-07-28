using DotVgn.Client.Mapper;
using DotVgn.Data.Contracts;
using DotVgn.Data.Enumerations;

namespace DotVgn.Tests.Client;

public sealed class StationMapperTests {
    private static StationResponseContract.StationContract CreateValidContract() => new() {
        Name = "Hauptbahnhof",
        VagId = "DE-12345",
        VgnId = 6789,
        Latitude = 49.4521,
        Longitude = 11.0778,
        Transports = "Bus,Tram,UBahn"
    };

    [Fact]
    public void Map_Single_ValidContract_MapsAllFields() {
        var mapper = new StationMapper();
        var contract = CreateValidContract();

        var result = mapper.Map(contract);

        Assert.Equal("Hauptbahnhof", result.Name);
        Assert.Equal(6789, result.StationId);
        Assert.Equal(49.4521, result.Latitude);
        Assert.Equal(11.0778, result.Longitude);
    }

    [Fact]
    public void Map_Single_Transports_ParsesCorrectly() {
        var mapper = new StationMapper();
        var contract = CreateValidContract() with { Transports = "Bus,Tram,UBahn" };

        var result = mapper.Map(contract);
        var transports = result.Transports.ToList();

        Assert.Equal(3, transports.Count);
        Assert.Contains(TransportType.Bus, transports);
        Assert.Contains(TransportType.Tram, transports);
        Assert.Contains(TransportType.UBahn, transports);
    }

    [Fact]
    public void Map_Single_TransportsNull_ReturnsUnknown() {
        var mapper = new StationMapper();
#pragma warning disable CS8625 // Transports is nullable string? in the contract
        var contract = new StationResponseContract.StationContract {
            Name = "Test",
            VagId = "X",
            VgnId = 1,
            Latitude = 49.0,
            Longitude = 11.0,
            Transports = null
        };
#pragma warning restore CS8625

        var result = mapper.Map(contract);

        var transport = Assert.Single(result.Transports);
        Assert.Equal(TransportType.Unknown, transport);
    }

    [Fact]
    public void Map_Single_TransportsEmpty_ReturnsUnknown() {
        var mapper = new StationMapper();
        var contract = CreateValidContract() with { Transports = "" };

        var result = mapper.Map(contract);

        var transport = Assert.Single(result.Transports);
        Assert.Equal(TransportType.Unknown, transport);
    }

    [Fact]
    public void Map_Single_TransportsWithUnknownType_DefaultsToUnknown() {
        var mapper = new StationMapper();
        var contract = CreateValidContract() with { Transports = "Bus,InvalidType,UBahn" };

        var result = mapper.Map(contract);
        var transports = result.Transports.ToList();

        Assert.Equal(3, transports.Count);
        Assert.Equal(TransportType.Bus, transports[0]);
        Assert.Equal(TransportType.Unknown, transports[1]);
        Assert.Equal(TransportType.UBahn, transports[2]);
    }

    [Fact]
    public void Map_Single_TransportsAllInvalid_AllUnknown() {
        var mapper = new StationMapper();
        var contract = CreateValidContract() with { Transports = "Foo,Bar,Baz" };

        var result = mapper.Map(contract);

        Assert.All(result.Transports, t => Assert.Equal(TransportType.Unknown, t));
    }

    [Fact]
    public void Map_Collection_Empty_ReturnsEmptyList() {
        var mapper = new StationMapper();

        var result = mapper.Map([]);

        Assert.Empty(result);
    }

    [Fact]
    public void Map_Single_TransportsWhitespaceEntries_SkipsEmpty() {
        var mapper = new StationMapper();
        var contract = CreateValidContract() with { Transports = "Bus,,Tram" };

        var result = mapper.Map(contract);
        var transports = result.Transports.ToList();

        Assert.Equal(2, transports.Count);
        Assert.Contains(TransportType.Bus, transports);
        Assert.Contains(TransportType.Tram, transports);
    }

    [Fact]
    public void Map_Single_TransportsCaseInsensitive_Parses() {
        var mapper = new StationMapper();
        var contract = CreateValidContract() with { Transports = "bus,TRAM,UBAHN" };

        var result = mapper.Map(contract);
        var transports = result.Transports.ToList();

        Assert.Equal(3, transports.Count);
        Assert.Contains(TransportType.Bus, transports);
        Assert.Contains(TransportType.Tram, transports);
        Assert.Contains(TransportType.UBahn, transports);
    }

    [Fact]
    public void Map_Single_AllTransportTypes_Parses() {
        var mapper = new StationMapper();
        var contract = CreateValidContract() with { Transports = "Bus,Tram,UBahn,SBahn,RBahn" };

        var result = mapper.Map(contract);
        var transports = result.Transports.ToList();

        Assert.Equal(5, transports.Count);
        Assert.Contains(TransportType.Bus, transports);
        Assert.Contains(TransportType.Tram, transports);
        Assert.Contains(TransportType.UBahn, transports);
        Assert.Contains(TransportType.SBahn, transports);
        Assert.Contains(TransportType.RBahn, transports);
    }
}
