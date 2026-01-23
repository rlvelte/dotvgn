using DotVgn.Client;
using DotVgn.Client.Additional;
using DotVgn.Models.Enumerations;
using DotVgn.Queries;
using Microsoft.Extensions.Logging.Abstractions;

namespace DotVgn.Tests.Client;

/// TODO: Testing strategy
/// - Unit: Verify DepartureQuery builds correct relative URLs for line-based and transport-based queries; validate argument rules.
/// - Integration (smoke, current): Resolve a real station via API, then request departures and assert non-null/basic shape.
/// - Next steps: Replace live calls with recorded HttpMessageHandler; add boundary tests for limit/timespan/delay; map enum parsing edge cases.
[TestClass]
public class DepartureTests {
    [TestMethod]
    public void DepartureQueryUrl_ForLine() {
        var q = new DepartureQuery(stationId: 123, line: "U1", limit: 5, timespan: 10, delay: 5);
        Assert.AreEqual("abfahrten/vgn/123/U1?timespan=10&timedelay=5&limitcount=5", q.GetRelativeUriExtension());
    }

    [TestMethod]
    public void DepartureQueryUrl_ForTransports() {
        var q = new DepartureQuery(stationId: 123, transports: [TransportType.UBahn, TransportType.Bus], limit: 20, timespan: 15, delay: 3);
        Assert.AreEqual("abfahrten/vgn/123?product=UBahn,Bus&timespan=15&timedelay=3&limitcount=20", q.GetRelativeUriExtension());
    }

    [TestMethod]
    public async Task GetDepartures_ForResolvedStation_Smoke() {
        var options = new DotVgnClientOptions();
        var client = new DotVgnClient(new HttpClient(new HttpClientHandler()), options, new NullLogger<DotVgnClient>());
        
        var stations = await client.GetStationsAsync(new StationQuery("Hauptbahnhof"));
        Assert.IsNotNull(stations);
        Assert.IsTrue(stations.Count > 0);

        var stationId = stations.First().StationId;
        var dQ = new DepartureQuery(stationId, [TransportType.UBahn], 5);
        var departures = await client.GetDeparturesAsync(dQ);
        Assert.IsNotNull(departures);
    }

    [TestMethod]
    public async Task GetDepartures_Batch_ForMultipleStations_Smoke() {
        var options = new DotVgnClientOptions();
        var client = new DotVgnClient(new HttpClient(new HttpClientHandler()), options, new NullLogger<DotVgnClient>());
        
        var stations = await client.GetStationsAsync(new StationQuery("Hauptbahnhof"));
        Assert.IsNotNull(stations);
        if (stations.Count == 0) return;

        var queries = stations.Select(s => new DepartureQuery(s.StationId, [TransportType.UBahn], 5)).ToList();
        var list = await client.GetDeparturesAsync(queries);

        Assert.IsNotNull(list);
    }
}
