using DotVgn.Queries;

namespace DotVgn.Tests.Client;

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
}
