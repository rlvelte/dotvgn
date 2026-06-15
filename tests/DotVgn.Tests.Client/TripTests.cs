using DotVgn.Client.Queries;
using DotVgn.Data.Enumerations;

namespace DotVgn.Tests.Client;

[TestClass]
public class TripTests {
    [TestMethod]
    public void TripQueryUrl_WithoutDate_ExactFormat() {
        var q = new TripQuery(TransportType.UBahn, 1234);
        var url = q.GetRelativeUriExtension();
        Assert.AreEqual("fahrten/UBahn/1234", url);
    }

    [TestMethod]
    public void TripQueryUrl_WithoutDate_Tram() {
        var q = new TripQuery(TransportType.Tram, 5678);
        Assert.AreEqual("fahrten/Tram/5678", q.GetRelativeUriExtension());
    }

    [TestMethod]
    public void TripQueryUrl_WithoutDate_Bus() {
        var q = new TripQuery(TransportType.Bus, 9012);
        Assert.AreEqual("fahrten/Bus/9012", q.GetRelativeUriExtension());
    }

    [TestMethod]
    public void TripQueryUrl_WithDate_ExactFormat() {
        var date = new DateTime(2024, 1, 2, 3, 4, 5);
        var q = new TripQuery(TransportType.Bus, 42, date);
        var url = q.GetRelativeUriExtension();
        Assert.AreEqual("fahrten/Bus/2024-01-02/42", url);
    }

    [TestMethod]
    public void TripQueryUrl_WithDate_Bus() {
        var q = new TripQuery(TransportType.Bus, 100, new DateTime(2024, 12, 24));
        Assert.AreEqual("fahrten/Bus/2024-12-24/100", q.GetRelativeUriExtension());
    }

    [TestMethod]
    public void TripQueryUrl_WithDate_Tram() {
        var q = new TripQuery(TransportType.Tram, 200, new DateTime(2024, 6, 15));
        Assert.AreEqual("fahrten/Tram/2024-06-15/200", q.GetRelativeUriExtension());
    }

    [TestMethod]
    public void TripQueryUrl_WithDate_UBahn() {
        var q = new TripQuery(TransportType.UBahn, 300, new DateTime(2025, 1, 1));
        Assert.AreEqual("fahrten/UBahn/2025-01-01/300", q.GetRelativeUriExtension());
    }

    [TestMethod]
    public void TripQueryUrl_WithDate_FirstDayOfMonth() {
        var q = new TripQuery(TransportType.Bus, 1, new DateTime(2024, 3, 1));
        Assert.AreEqual("fahrten/Bus/2024-03-01/1", q.GetRelativeUriExtension());
    }

    [TestMethod]
    public void TripQueryUrl_WithDate_LastDayOfYear() {
        var q = new TripQuery(TransportType.Tram, 999, new DateTime(2024, 12, 31));
        Assert.AreEqual("fahrten/Tram/2024-12-31/999", q.GetRelativeUriExtension());
    }
}
