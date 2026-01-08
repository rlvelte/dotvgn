<img src="https://i.imgur.com/hwplHVj.png" width=200>

# DotVGN - .NET client for the VGN/VAG API
Pragmatic client to query the VGN/VAG public transport API for:
- Stations (by name or geo)
- Departures (by line or by transport types)
- Trips (including all stops)
- Batch requests with internal throttling
- Optional in-memory caching of stations

> [!IMPORTANT]
> This package was extracted from a personal project. Ongoing development is driven by my perceived needs rather than a fixed roadmap. Contributions, improvements, and forks are very welcome.

## Install
The easiest way to get started is to install the package from [GitHub Packages](https://docs.github.com/de/packages/working-with-a-github-packages-registry/working-with-the-nuget-registry):
```
dotnet add package dotvgn
```

## Usage
The package contains helper methods for easy use dependency injection.

```csharp
// Minimal setup. You can also bind from configuration.
builder.Services.AddDotVgnClient((sp, opt) => {
    opt.BaseUri = new Uri("https://start.vag.de/dm/api/v1/");
});
```