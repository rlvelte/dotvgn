using dotVGN.Client;
using dotVGN.Mapper;
using dotVGN.Models.Enumerations;
using dotVGN.Queries;
using Microsoft.Extensions.Logging.Abstractions;

namespace DotVgn.Tests.ClientTests;

[TestClass]
public class QueryTests {
    [TestMethod]
    public void StationQueryUrlForName() {
        var query = new StationQuery("Hauptbahnhof");
        var url = query.GetRelativeUriExtension();

        Assert.AreEqual("haltestellen/vgn?name=Hauptbahnhof", url);
    }

    [TestMethod]
    public void StationQueryUrlForGeo() {
        var query = new StationQuery(49.0, 11.0, 300);
        var url = query.GetRelativeUriExtension();

        Assert.AreEqual("haltestellen/vgn/location?lon=11&lat=49&radius=300", url);
    }

    [TestMethod]
    public async Task GetStationByName() {
        var query = new StationQuery("Hauptbahnhof");
        var options = new DotVgnClientOptions();

        var client = new DotVgnClient(new HttpClient(new HttpClientHandler()), options, new NullLogger<DotVgnClient>(), new StationMapper(), new DepartureMapper());
        var result = await client.GetStationsAsync(query);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Count > 0);
    }

    [TestMethod]
    public async Task GetDeparturesForInvalidStation() {
        var query = new StationQuery("StationThatDoesNotExist123");
        var options = new DotVgnClientOptions();

        var client = new DotVgnClient(new HttpClient(new HttpClientHandler()), options, new NullLogger<DotVgnClient>(), new StationMapper(), new DepartureMapper());
        var result = await client.GetStationsAsync(query);

        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task GetStationsInRadius() {
        var query = new StationQuery(49.446096, 11.087705, 250);
        var options = new DotVgnClientOptions();

        var client = new DotVgnClient(new HttpClient(new HttpClientHandler()), options, new NullLogger<DotVgnClient>(), new StationMapper(), new DepartureMapper());
        var result = await client.GetStationsAsync(query);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Count > 0);
    }

    [TestMethod]
    public async Task GetDeparturesForValidStation() {
        var sQ = new StationQuery("Hauptbahnhof");
        var options = new DotVgnClientOptions();

        var client = new DotVgnClient(
            new HttpClient(new HttpClientHandler()),
            options,
            new NullLogger<DotVgnClient>(),
            new StationMapper(),
            new DepartureMapper());

        var stations = await client.GetStationsAsync(sQ);
        Assert.IsNotNull(stations);
        Assert.IsTrue(stations.Count > 0);

        var dQ = new DepartureQuery(stations.First().StationId, [TransportType.UBahn], 5);
        var departures = await client.GetDeparturesAsync(dQ);
        Assert.IsNotNull(departures);
    }
}