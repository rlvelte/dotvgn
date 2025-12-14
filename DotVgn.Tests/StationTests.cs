using DotVgn.Client;
using DotVgn.Client.Additional;
using DotVgn.Queries;
using Microsoft.Extensions.Logging.Abstractions;

namespace DotVgn.Tests;

/// TODO: Testing strategy
/// - Unit: Verify StationQuery builds correct relative URLs for name and geo queries.
/// - Integration (smoke, current): Call live API via DotVgnClient to fetch stations by name and geo and assert basic invariants.
/// - Next steps: Replace live calls with HttpMessageHandler stubs/recordings; add edge cases (empty, special chars), caching behavior tests with short TTL.
[TestClass]
public class StationTests {
    [TestMethod]
    public void StationQueryUrl_ForName() {
        var query = new StationQuery("Hauptbahnhof");
        var url = query.GetRelativeUriExtension();
        Assert.AreEqual("haltestellen/vgn?name=Hauptbahnhof", url);
    }

    [TestMethod]
    public void StationQueryUrl_ForGeo() {
        var query = new StationQuery(49.0, 11.0, 300);
        var url = query.GetRelativeUriExtension();
        Assert.AreEqual("haltestellen/vgn/location?lon=11&lat=49&radius=300", url);
    }

    [TestMethod]
    public async Task GetStations_ByName_Smoke() {
        var query = new StationQuery("Hauptbahnhof");
        var options = new DotVgnClientOptions();
        var client = new DotVgnClient(new HttpClient(new HttpClientHandler()), options, new NullLogger<DotVgnClient>());

        var result = await client.GetStationsAsync(query);
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Count > 0);
    }

    [TestMethod]
    public async Task GetStations_InvalidName_ReturnsEmpty() {
        var query = new StationQuery("StationThatDoesNotExist123");
        var options = new DotVgnClientOptions();
        var client = new DotVgnClient(new HttpClient(new HttpClientHandler()), options, new NullLogger<DotVgnClient>());

        var result = await client.GetStationsAsync(query);
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task GetStations_InRadius_Smoke() {
        var query = new StationQuery(49.446096, 11.087705, 250);
        var options = new DotVgnClientOptions();
        var client = new DotVgnClient(new HttpClient(new HttpClientHandler()), options, new NullLogger<DotVgnClient>());

        var result = await client.GetStationsAsync(query);
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Count > 0);
    }

    [TestMethod]
    public async Task GetStations_Batch_MixedQueries_Smoke() {
        var options = new DotVgnClientOptions();
        var client = new DotVgnClient(new HttpClient(new HttpClientHandler()), options, new NullLogger<DotVgnClient>());

        var queries = new List<StationQuery> {
            new("Hauptbahnhof"),
            new(49.446096, 11.087705, 250)
        };

        var results = await client.GetStationsAsync(queries);
        Assert.IsNotNull(results);
        Assert.IsTrue(results.Count > 0);
        foreach (var (q, stations) in results) {
            Assert.IsNotNull(q);
            Assert.IsNotNull(stations);
        }
    }
}
