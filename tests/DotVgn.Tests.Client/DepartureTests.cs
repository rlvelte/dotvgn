using DotVgn.Data.Enumerations;
using DotVgn.Queries;

namespace DotVgn.Tests.Client;

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
}
