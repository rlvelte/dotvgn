using DotVgn.Client;
using DotVgn.Client.Additional;
using DotVgn.Models.Enumerations;
using DotVgn.Queries;
using Microsoft.Extensions.Logging.Abstractions;

namespace DotVgn.Tests.Client;

/// TODO: Testing strategy
/// - Unit: Verify TripQuery builds correct relative URLs with and without date. Avoid brittle date formatting assertions.
/// - Integration (smoke, current): Resolve a departure, then fetch the corresponding trip and assert basic invariants (non-null, has stops).
/// - Next steps: Stub HTTP and add detailed mapping tests for TripMapper (stop fields, time conversions, platform), edge cases (no stops, missing fields).
[TestClass]
public class TripTests {
    [TestMethod]
    public void TripQueryUrl_WithoutDate() {
        var q = new TripQuery(TransportType.UBahn, 1234);
        var url = q.GetRelativeUriExtension();
        StringAssert.StartsWith(url, "fahrten/UBahn/");
        StringAssert.EndsWith(url, "/1234");
    }

    [TestMethod]
    public void TripQueryUrl_WithDate() {
        var date = new DateTime(2024, 1, 2, 3, 4, 5);
        var q = new TripQuery(TransportType.Bus, 42, date);
        var url = q.GetRelativeUriExtension();
        
        StringAssert.StartsWith(url, "fahrten/Bus/");
        StringAssert.Contains(url, "42");
    }

    [TestMethod]
    public async Task GetTrip_FromDeparture_Smoke() {
        var options = new DotVgnClientOptions();
        var client = new DotVgnClient(new HttpClient(new HttpClientHandler()), options, new NullLogger<DotVgnClient>());
        
        var stations = await client.GetStationsAsync(new StationQuery("Hauptbahnhof"));
        Assert.IsNotNull(stations);
        Assert.IsTrue(stations.Count > 0);

        var stationId = stations.First().StationId;
        var departures = await client.GetDeparturesAsync(new DepartureQuery(stationId, [TransportType.UBahn], 3));
        Assert.IsNotNull(departures);
        if (departures.Count == 0) return; 

        var d = departures.First();
        var trip = await client.GetTripAsync(new TripQuery(d.TransportType, d.TripNumber));
        if (trip is not null) {
            Assert.IsFalse(string.IsNullOrWhiteSpace(trip.Line));
            Assert.IsTrue(trip.Stops is not null);
        }
    }

    [TestMethod]
    public async Task GetTrips_Batch_FromDepartures_Smoke() {
        var options = new DotVgnClientOptions();
        var client = new DotVgnClient(new HttpClient(new HttpClientHandler()), options, new NullLogger<DotVgnClient>());

        var stations = await client.GetStationsAsync(new StationQuery("Hauptbahnhof"));
        if (stations.Count == 0) return;

        var stationId = stations.First().StationId;
        var departures = await client.GetDeparturesAsync(new DepartureQuery(stationId, [TransportType.UBahn], 5));
        if (departures.Count == 0) return;

        var tripQueries = departures
            .Select(d => new TripQuery(d.TransportType, d.TripNumber))
            .DistinctBy(q => (q.GetRelativeUriExtension()))
            .Take(3)
            .ToList();

        var results = await client.GetTripsAsync(tripQueries);
        Assert.IsNotNull(results);
        foreach (var (q, trips) in results) {
            Assert.IsNotNull(q);
            Assert.IsNotNull(trips);
            
            Assert.IsTrue(trips.Count <= 1);
        }
    }
}
