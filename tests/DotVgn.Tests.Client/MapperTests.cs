using DotVgn.Client.Mapper;
using DotVgn.Data.Contracts;
using DotVgn.Data.Enumerations;

namespace DotVgn.Tests.Client;

[TestClass]
public sealed class DepartureMapperTests {
    private static DepartureResponseContract.DepartureContract CreateValidContract() => new() {
        Line = "U1",
        StopPoint = "Platform 1",
        Direction = "Richtung1",
        DirectionDescription = "Fürth Hauptbahnhof",
        Date = new DateTimeOffset(2024, 1, 15, 0, 0, 0, TimeSpan.Zero),
        DepartureTimePlanned = new DateTimeOffset(2024, 1, 15, 14, 30, 0, TimeSpan.FromHours(1)),
        DepartureTimeActual = new DateTimeOffset(2024, 1, 15, 14, 32, 0, TimeSpan.FromHours(1)),
        Transport = "Bus",
        TripNumber = 12345,
        OccupationLevel = "low"
    };

    [TestMethod]
    public void Map_Single_ValidContract_MapsAllFields() {
        var mapper = new DepartureMapper();
        var contract = CreateValidContract();

        var result = mapper.Map(contract);

        Assert.AreEqual("U1", result.Line);
        Assert.AreEqual("Platform 1", result.StopPoint);
        Assert.AreEqual("Richtung1", result.Direction);
        Assert.AreEqual("Fürth Hauptbahnhof", result.DirectionDescription);
        Assert.AreEqual(TransportType.Bus, result.TransportType);
        Assert.AreEqual(12345, result.TripNumber);
        Assert.AreEqual("low", result.OccupationLevel);
        Assert.AreEqual(14, result.DepartureTimePlanned.Hour);
        Assert.AreEqual(30, result.DepartureTimePlanned.Minute);
    }

    [TestMethod]
    public void Map_Single_TransportBus_ParsesCorrectly() {
        var mapper = new DepartureMapper();
        var contract = CreateValidContract() with { Transport = "Bus" };

        var result = mapper.Map(contract);

        Assert.AreEqual(TransportType.Bus, result.TransportType);
    }

    [TestMethod]
    public void Map_Single_TransportTram_ParsesCorrectly() {
        var mapper = new DepartureMapper();
        var contract = CreateValidContract() with { Transport = "Tram" };

        var result = mapper.Map(contract);

        Assert.AreEqual(TransportType.Tram, result.TransportType);
    }

    [TestMethod]
    public void Map_Single_TransportUBahn_ParsesCorrectly() {
        var mapper = new DepartureMapper();
        var contract = CreateValidContract() with { Transport = "UBahn" };

        var result = mapper.Map(contract);

        Assert.AreEqual(TransportType.UBahn, result.TransportType);
    }

    [TestMethod]
    public void Map_Single_TransportUnknown_FallsBackToUnknown() {
        var mapper = new DepartureMapper();
        var contract = CreateValidContract() with { Transport = "UnknownType" };

        var result = mapper.Map(contract);

        Assert.AreEqual(TransportType.Unknown, result.TransportType);
    }

    [TestMethod]
    public void Map_Single_TransportEmpty_FallsBackToUnknown() {
        var mapper = new DepartureMapper();
        var contract = CreateValidContract() with { Transport = "" };

        var result = mapper.Map(contract);

        Assert.AreEqual(TransportType.Unknown, result.TransportType);
    }

    [TestMethod]
    public void Map_Single_TransportWhitespace_FallsBackToUnknown() {
        var mapper = new DepartureMapper();
        var contract = CreateValidContract() with { Transport = "  " };

        var result = mapper.Map(contract);

        Assert.AreEqual(TransportType.Unknown, result.TransportType);
    }

    [TestMethod]
    public void Map_Collection_Empty_ReturnsEmptyList() {
        var mapper = new DepartureMapper();

        var result = mapper.Map(Array.Empty<DepartureResponseContract.DepartureContract>());

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public void Map_Collection_MultipleContracts_MapsAll() {
        var mapper = new DepartureMapper();
        var contracts = new[] {
            CreateValidContract() with { Line = "U1", Transport = "UBahn" },
            CreateValidContract() with { Line = "U2", Transport = "UBahn" },
            CreateValidContract() with { Line = "Bus 35", Transport = "Bus" }
        };

        var result = mapper.Map(contracts);

        Assert.AreEqual(3, result.Count);
        Assert.AreEqual("U1", result[0].Line);
        Assert.AreEqual("U2", result[1].Line);
        Assert.AreEqual("Bus 35", result[2].Line);
    }

    [TestMethod]
    public void Map_Single_DateTimeOffset_ConvertsToDateTime() {
        var mapper = new DepartureMapper();
        var contract = CreateValidContract() with {
            DepartureTimeActual = new DateTimeOffset(2024, 6, 15, 8, 45, 0, TimeSpan.FromHours(2)),
            DepartureTimePlanned = new DateTimeOffset(2024, 6, 15, 8, 40, 0, TimeSpan.FromHours(2))
        };

        var result = mapper.Map(contract);

        Assert.AreEqual(8, result.DepartureTimePlanned.Hour);
        Assert.AreEqual(40, result.DepartureTimePlanned.Minute);
        Assert.AreEqual(8, result.DepartureTimeActual.Hour);
        Assert.AreEqual(45, result.DepartureTimeActual.Minute);
    }

    [TestMethod]
    public void Map_Single_TripNumberDefaultZero() {
        var mapper = new DepartureMapper();
        var contract = CreateValidContract() with { TripNumber = 0 };

        var result = mapper.Map(contract);

        Assert.AreEqual(0, result.TripNumber);
    }
}

[TestClass]
public sealed class StationMapperTests {
    private static StationResponseContract.StationContract CreateValidContract() => new() {
        Name = "Hauptbahnhof",
        VagId = "DE-12345",
        VgnId = 6789,
        Latitude = 49.4521,
        Longitude = 11.0778,
        Transports = "Bus,Tram,UBahn"
    };

    [TestMethod]
    public void Map_Single_ValidContract_MapsAllFields() {
        var mapper = new StationMapper();
        var contract = CreateValidContract();

        var result = mapper.Map(contract);

        Assert.AreEqual("Hauptbahnhof", result.Name);
        Assert.AreEqual(6789, result.StationId);
        Assert.AreEqual(49.4521, result.Latitude);
        Assert.AreEqual(11.0778, result.Longitude);
    }

    [TestMethod]
    public void Map_Single_Transports_ParsesCorrectly() {
        var mapper = new StationMapper();
        var contract = CreateValidContract() with { Transports = "Bus,Tram,UBahn" };

        var result = mapper.Map(contract);

        var transports = result.Transports.ToList();
        Assert.AreEqual(3, transports.Count);
        Assert.IsTrue(transports.Contains(TransportType.Bus));
        Assert.IsTrue(transports.Contains(TransportType.Tram));
        Assert.IsTrue(transports.Contains(TransportType.UBahn));
    }

    [TestMethod]
    public void Map_Single_TransportsNull_ReturnsUnknown() {
        var mapper = new StationMapper();
#pragma warning disable CS8625 // Transports is nullable string? in the contract
        var contract = new StationResponseContract.StationContract {
            Name = "Test", VagId = "X", VgnId = 1,
            Latitude = 49.0, Longitude = 11.0,
            Transports = null
        };
#pragma warning restore CS8625

        var result = mapper.Map(contract);

        var transports = result.Transports.ToList();
        Assert.AreEqual(1, transports.Count);
        Assert.AreEqual(TransportType.Unknown, transports[0]);
    }

    [TestMethod]
    public void Map_Single_TransportsEmpty_ReturnsUnknown() {
        var mapper = new StationMapper();
        var contract = CreateValidContract() with { Transports = "" };

        var result = mapper.Map(contract);

        var transports = result.Transports.ToList();
        Assert.AreEqual(1, transports.Count);
        Assert.AreEqual(TransportType.Unknown, transports[0]);
    }

    [TestMethod]
    public void Map_Single_TransportsWithUnknownType_DefaultsToUnknown() {
        var mapper = new StationMapper();
        var contract = CreateValidContract() with { Transports = "Bus,InvalidType,UBahn" };

        var result = mapper.Map(contract);

        var transports = result.Transports.ToList();
        Assert.AreEqual(3, transports.Count);
        Assert.AreEqual(TransportType.Bus, transports[0]);
        Assert.AreEqual(TransportType.Unknown, transports[1]);
        Assert.AreEqual(TransportType.UBahn, transports[2]);
    }

    [TestMethod]
    public void Map_Single_TransportsAllInvalid_AllUnknown() {
        var mapper = new StationMapper();
        var contract = CreateValidContract() with { Transports = "Foo,Bar,Baz" };

        var result = mapper.Map(contract);

        Assert.IsTrue(result.Transports.All(t => t == TransportType.Unknown));
    }

    [TestMethod]
    public void Map_Collection_Empty_ReturnsEmptyList() {
        var mapper = new StationMapper();

        var result = mapper.Map([]);

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public void Map_Single_TransportsWhitespaceEntries_SkipsEmpty() {
        var mapper = new StationMapper();
        var contract = CreateValidContract() with { Transports = "Bus,,Tram" };

        var result = mapper.Map(contract);

        var transports = result.Transports.ToList();
        Assert.AreEqual(2, transports.Count);
        Assert.IsTrue(transports.Contains(TransportType.Bus));
        Assert.IsTrue(transports.Contains(TransportType.Tram));
    }

    [TestMethod]
    public void Map_Single_TransportsCaseInsensitive_Parses() {
        var mapper = new StationMapper();
        var contract = CreateValidContract() with { Transports = "bus,TRAM,UBAHN" };

        var result = mapper.Map(contract);

        var transports = result.Transports.ToList();
        Assert.AreEqual(3, transports.Count);
        Assert.IsTrue(transports.Contains(TransportType.Bus));
        Assert.IsTrue(transports.Contains(TransportType.Tram));
        Assert.IsTrue(transports.Contains(TransportType.UBahn));
    }

    [TestMethod]
    public void Map_Single_AllTransportTypes_Parses() {
        var mapper = new StationMapper();
        var contract = CreateValidContract() with { Transports = "Bus,Tram,UBahn,SBahn,RBahn" };

        var result = mapper.Map(contract);

        var transports = result.Transports.ToList();
        Assert.AreEqual(5, transports.Count);
        Assert.IsTrue(transports.Contains(TransportType.Bus));
        Assert.IsTrue(transports.Contains(TransportType.Tram));
        Assert.IsTrue(transports.Contains(TransportType.UBahn));
        Assert.IsTrue(transports.Contains(TransportType.SBahn));
        Assert.IsTrue(transports.Contains(TransportType.RBahn));
    }
}

[TestClass]
public sealed class TripMapperTests {
    private static MetadataContract CreateDummyMetadata() => new() {
        Version = "1.0",
        Timestamp = "2024-01-15T12:00:00Z"
    };

    private static TripResponseContract CreateValidContract() => new() {
        Metadata = CreateDummyMetadata(),
        Line = "U1",
        Direction = "Richtung1",
        DirectionDescription = "Fürth Hauptbahnhof",
        Stops = [
            new TripResponseContract.StopContract {
                Name = "Hauptbahnhof",
                VagId = "DE-123",
                VgnId = 456,
                Platform = "1",
                Latitude = 49.4521,
                Longitude = 11.0778,
                ArrivalTimeEstimated = new DateTime(2024, 1, 15, 14, 30, 0),
                ArrivalTimeActual = new DateTime(2024, 1, 15, 14, 32, 0),
                DepartureTimeEstimated = new DateTime(2024, 1, 15, 14, 31, 0),
                DepartureTimeActual = new DateTime(2024, 1, 15, 14, 33, 0)
            }
        ]
    };

    [TestMethod]
    public void Map_ValidContract_MapsAllFields() {
        var mapper = new TripMapper();
        var contract = CreateValidContract();

        var result = mapper.Map(contract);

        Assert.AreEqual("U1", result.Line);
        Assert.AreEqual("Richtung1", result.Direction);
        Assert.AreEqual("Fürth Hauptbahnhof", result.DirectionDescription);
    }

    [TestMethod]
    public void Map_MultipleStops_MapsAll() {
        var mapper = new TripMapper();
        var contract = CreateValidContract() with {
            Stops = [
                new TripResponseContract.StopContract {
                    Name = "Stop A", VagId = "A1", VgnId = 1, Platform = "1",
                    Latitude = 49.0, Longitude = 11.0
                },
                new TripResponseContract.StopContract {
                    Name = "Stop B", VagId = "B2", VgnId = 2, Platform = "2",
                    Latitude = 49.5, Longitude = 11.5
                },
                new TripResponseContract.StopContract {
                    Name = "Stop C", VagId = "C3", VgnId = 3, Platform = "3",
                    Latitude = 50.0, Longitude = 12.0
                }
            ]
        };

        var result = mapper.Map(contract);

        Assert.AreEqual(3, result.Stops.Count());
        Assert.AreEqual("Stop A", result.Stops.ElementAt(0).StationName);
        Assert.AreEqual("Stop B", result.Stops.ElementAt(1).StationName);
        Assert.AreEqual("Stop C", result.Stops.ElementAt(2).StationName);
    }

    [TestMethod]
    public void Map_NoStops_ReturnsEmptyStops() {
        var mapper = new TripMapper();
        var contract = CreateValidContract() with {
            Stops = []
        };

        var result = mapper.Map(contract);

        Assert.AreEqual(0, result.Stops.Count());
    }

    [TestMethod]
    public void Map_StopContract_MapsAllFields() {
        var mapper = new TripMapper();
        var contract = CreateValidContract() with {
            Stops = [
                new TripResponseContract.StopContract {
                    Name = "Test Station",
                    VagId = "DE-TEST",
                    VgnId = 9999,
                    Platform = "Platform 3",
                    Latitude = 49.1234,
                    Longitude = 11.5678,
                    ArrivalTimeEstimated = new DateTime(2024, 6, 1, 10, 0, 0),
                    ArrivalTimeActual = new DateTime(2024, 6, 1, 10, 5, 0),
                    DepartureTimeEstimated = new DateTime(2024, 6, 1, 10, 1, 0),
                    DepartureTimeActual = new DateTime(2024, 6, 1, 10, 6, 0)
                }
            ]
        };

        var result = mapper.Map(contract);

        var stop = result.Stops.Single();
        Assert.AreEqual("Test Station", stop.StationName);
        Assert.AreEqual(9999, stop.StationId);
        Assert.AreEqual("Platform 3", stop.Platform);
        Assert.AreEqual(49.1234, stop.Latitude);
        Assert.AreEqual(11.5678, stop.Longitude);
        Assert.IsNotNull(stop.ArrivalTimeEstimated);
        Assert.IsNotNull(stop.ArrivalTimeActual);
        Assert.IsNotNull(stop.DepartureTimeEstimated);
        Assert.IsNotNull(stop.DepartureTimeActual);
    }

    [TestMethod]
    public void Map_StopContract_NullableTimes_Null() {
        var mapper = new TripMapper();
        var contract = CreateValidContract() with {
            Stops = [
                new TripResponseContract.StopContract {
                    Name = "Station",
                    VagId = "DE-1",
                    VgnId = 1,
                    Platform = "1",
                    Latitude = 49.0,
                    Longitude = 11.0
                }
            ]
        };

        var result = mapper.Map(contract);

        var stop = result.Stops.Single();
        Assert.IsNull(stop.ArrivalTimeEstimated);
        Assert.IsNull(stop.ArrivalTimeActual);
        Assert.IsNull(stop.DepartureTimeEstimated);
        Assert.IsNull(stop.DepartureTimeActual);
    }
}
