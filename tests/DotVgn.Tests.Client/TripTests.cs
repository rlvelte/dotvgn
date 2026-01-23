using DotVgn.Data.Enumerations;
using DotVgn.Queries;

namespace DotVgn.Tests.Client;

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
}
