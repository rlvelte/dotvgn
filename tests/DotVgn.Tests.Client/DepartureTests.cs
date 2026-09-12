using DotVgn.Client.Queries;
using DotVgn.Common.Enumerations;

namespace DotVgn.Tests.Client;

public class DepartureTests {
    [Fact]
    public void DepartureQueryUrl_ForLine_Basic() {
        var q = new DepartureQuery(stationId: 123, line: "U1", limit: 5, timespan: 10, delay: 5);
        Assert.Equal("abfahrten/vgn/123/U1?timespan=10&timedelay=5&limitcount=5", q.GetRelativeUriExtension());
    }

    [Fact]
    public void DepartureQueryUrl_ForLine_DefaultParameters() {
        var q = new DepartureQuery(stationId: 456, line: "S1", limit: 10);
        Assert.Equal("abfahrten/vgn/456/S1?timespan=10&timedelay=5&limitcount=10", q.GetRelativeUriExtension());
    }

    [Fact]
    public void DepartureQueryUrl_ForLine_CustomLine() {
        var q = new DepartureQuery(stationId: 789, line: "N11", limit: 3, timespan: 30, delay: 0);
        Assert.Equal("abfahrten/vgn/789/N11?timespan=30&timedelay=0&limitcount=3", q.GetRelativeUriExtension());
    }

    [Fact]
    public void DepartureQueryUrl_ForTransports_Multiple() {
        var q = new DepartureQuery(stationId: 123, transports: [TransportType.UBahn, TransportType.Bus], limit: 20, timespan: 15, delay: 3);
        Assert.Equal("abfahrten/vgn/123?product=UBahn,Bus&timespan=15&timedelay=3&limitcount=20", q.GetRelativeUriExtension());
    }

    [Fact]
    public void DepartureQueryUrl_ForTransports_Single() {
        var q = new DepartureQuery(stationId: 999, transports: [TransportType.Tram], limit: 5);
        Assert.Equal("abfahrten/vgn/999?product=Tram&timespan=10&timedelay=5&limitcount=5", q.GetRelativeUriExtension());
    }

    [Fact]
    public void DepartureQueryUrl_ForTransports_UnknownFiltered() {
        // TransportType.Unknown should be filtered out by ParseTransports
        var q = new DepartureQuery(stationId: 1, transports: [TransportType.UBahn, TransportType.Unknown, TransportType.Bus], limit: 10);
        Assert.Equal("abfahrten/vgn/1?product=UBahn,Bus&timespan=10&timedelay=5&limitcount=10", q.GetRelativeUriExtension());
    }

    [Fact]
    public void DepartureQueryUrl_ForTransports_AllValidTypes() {
        var q = new DepartureQuery(stationId: 555, transports: [TransportType.UBahn, TransportType.Bus, TransportType.Tram, TransportType.SBahn, TransportType.RBahn], limit: 30, timespan: 20, delay: 2);
        Assert.Equal("abfahrten/vgn/555?product=UBahn,Bus,Tram,SBahn,RBahn&timespan=20&timedelay=2&limitcount=30", q.GetRelativeUriExtension());
    }

    [Fact]
    public void DepartureQueryUrl_ForTransports_OnlyUnknown_EmptyProduct() {
        // TransportType.Unknown only should result in empty product parameter
        var q = new DepartureQuery(stationId: 1, transports: [TransportType.Unknown], limit: 5);
        Assert.Equal("abfahrten/vgn/1?product=&timespan=10&timedelay=5&limitcount=5", q.GetRelativeUriExtension());
    }

    [Fact]
    public void DepartureQueryUrl_ForLine_StationIdZero() {
        var q = new DepartureQuery(stationId: 0, line: "U1", limit: 1);
        Assert.Equal("abfahrten/vgn/0/U1?timespan=10&timedelay=5&limitcount=1", q.GetRelativeUriExtension());
    }

    [Fact]
    public void DepartureQueryUrl_ForLine_LargeLimit() {
        var q = new DepartureQuery(stationId: 100, line: "R2", limit: 999);
        Assert.Equal("abfahrten/vgn/100/R2?timespan=10&timedelay=5&limitcount=999", q.GetRelativeUriExtension());
    }

    [Fact]
    public void DepartureQuery_Constructor_EmptyTransports_Throws() {
        Assert.Throws<ArgumentException>(() => new DepartureQuery(stationId: 1, transports: [], limit: 5));
    }
}
