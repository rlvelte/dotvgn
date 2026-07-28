using DotVgn.Client.Mapper;
using DotVgn.Data.Contracts;

namespace DotVgn.Tests.Client;

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

    [Fact]
    public void Map_ValidContract_MapsAllFields() {
        var mapper = new TripMapper();
        var contract = CreateValidContract();

        var result = mapper.Map(contract);

        Assert.Equal("U1", result.Line);
        Assert.Equal("Richtung1", result.Direction);
        Assert.Equal("Fürth Hauptbahnhof", result.DirectionDescription);
    }

    [Fact]
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

        Assert.Equal(3, result.Stops.Count());
        Assert.Equal("Stop A", result.Stops.ElementAt(0).StationName);
        Assert.Equal("Stop B", result.Stops.ElementAt(1).StationName);
        Assert.Equal("Stop C", result.Stops.ElementAt(2).StationName);
    }

    [Fact]
    public void Map_NoStops_ReturnsEmptyStops() {
        var mapper = new TripMapper();
        var contract = CreateValidContract() with {
            Stops = []
        };

        var result = mapper.Map(contract);

        Assert.Empty(result.Stops);
    }

    [Fact]
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
        Assert.Equal("Test Station", stop.StationName);
        Assert.Equal(9999, stop.StationId);
        Assert.Equal("Platform 3", stop.Platform);
        Assert.Equal(49.1234, stop.Latitude);
        Assert.Equal(11.5678, stop.Longitude);
        Assert.NotNull(stop.ArrivalTimeEstimated);
        Assert.NotNull(stop.ArrivalTimeActual);
        Assert.NotNull(stop.DepartureTimeEstimated);
        Assert.NotNull(stop.DepartureTimeActual);
    }

    [Fact]
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
        Assert.Null(stop.ArrivalTimeEstimated);
        Assert.Null(stop.ArrivalTimeActual);
        Assert.Null(stop.DepartureTimeEstimated);
        Assert.Null(stop.DepartureTimeActual);
    }
}
