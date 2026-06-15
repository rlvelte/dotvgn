using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;
using DotVgn.Client.Queries;

namespace DotVgn.Tests.Client;

[TestClass]
public class StationTests {
    [TestMethod]
    public void StationQueryUrl_ForName_Simple() {
        var query = new StationQuery("Hauptbahnhof");
        var url = query.GetRelativeUriExtension();
        Assert.AreEqual("haltestellen/vgn?name=Hauptbahnhof", url);
    }

    [TestMethod]
    public void StationQueryUrl_ForName_MultipleWords() {
        var query = new StationQuery("Nürnberg Hbf");
        var url = query.GetRelativeUriExtension();
        Assert.AreEqual("haltestellen/vgn?name=N%C3%BCrnberg%20Hbf", url);
    }

    [TestMethod]
    public void StationQueryUrl_ForName_SpecialCharacters() {
        var query = new StationQuery("München-Schwabing");
        var url = query.GetRelativeUriExtension();
        Assert.AreEqual("haltestellen/vgn?name=M%C3%BCnchen-Schwabing", url);
    }

    [TestMethod]
    public void StationQueryUrl_ForName_Umlauts() {
        var query = new StationQuery("Fürth Hbf");
        var url = query.GetRelativeUriExtension();
        Assert.AreEqual("haltestellen/vgn?name=F%C3%BCrth%20Hbf", url);
    }

    [TestMethod]
    public void StationQueryUrl_ForGeo_WholeNumbers() {
        var query = new StationQuery(49.0, 11.0, 300);
        var url = query.GetRelativeUriExtension();
        Assert.AreEqual("haltestellen/vgn/location?lon=11&lat=49&radius=300", url);
    }

    [TestMethod]
    public void StationQueryUrl_ForGeo_FractionalCoordinates() {
        var query = new StationQuery(49.4521, 11.0778, 500);
        var url = query.GetRelativeUriExtension();
        Assert.AreEqual("haltestellen/vgn/location?lon=11.0778&lat=49.4521&radius=500", url);
    }

    [TestMethod]
    public void StationQueryUrl_ForGeo_NegativeLatitude() {
        var query = new StationQuery(-33.8568, 151.2153, 1000);
        var url = query.GetRelativeUriExtension();
        Assert.AreEqual("haltestellen/vgn/location?lon=151.2153&lat=-33.8568&radius=1000", url);
    }

    [TestMethod]
    public void StationQueryUrl_ForGeo_SmallRadius() {
        var query = new StationQuery(49.5, 11.5, 1);
        var url = query.GetRelativeUriExtension();
        Assert.AreEqual("haltestellen/vgn/location?lon=11.5&lat=49.5&radius=1", url);
    }

    [TestMethod]
    public void StationQueryUrl_ForGeo_LargeRadius() {
        var query = new StationQuery(49.5, 11.5, 50000);
        var url = query.GetRelativeUriExtension();
        Assert.AreEqual("haltestellen/vgn/location?lon=11.5&lat=49.5&radius=50000", url);
    }

    [TestMethod]
    public void StationQueryUrl_ForGeo_HighPrecision() {
        var query = new StationQuery(49.123456789, 11.987654321, 250);
        var url = query.GetRelativeUriExtension();
        Assert.AreEqual("haltestellen/vgn/location?lon=11.987654321&lat=49.123456789&radius=250", url);
    }
}
