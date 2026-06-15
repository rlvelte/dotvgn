<img src="https://i.imgur.com/hwplHVj.png" width=200>

# DotVGN - .NET Client for VAG/VGN API
![NuGet](https://img.shields.io/nuget/v/DotVgn?label=DotVgn&style=flat)

Lightweight .NET client for the VGN/VAG public transport API.

> [!IMPORTANT]
> This project originated from a personal application. Development follows my own needs rather than a fixed roadmap. Contributions and forks are welcome.


## Table of Contents
- [Install](#install)
- [Usage](#usage)
- [Samples](#samples)


## Install
The easiest way to get started is to install the package from [NuGet](https://www.nuget.org/) or [GitHub Packages](https://docs.github.com/de/packages/working-with-a-github-packages-registry/working-with-the-nuget-registry):
```
dotnet add package dotvgn
```

## Usage
### Dependency Injection
The package integrate seamlessly with `ServiceCollection`.

```csharp
// Provide another base endpoint
services.AddDotVgnClient(opt => {
    opt.BaseEndpoint = new Uri(...) // Optional
});
```

### Manual Initialization
Create client instances directly without dependency injection:
```csharp
// No arguments to use defaults
var client = new VgnClient();
```

## Samples
### Departure Monitor
A terminal based departure board. Shows real-time departures grouped by transport type (U-Bahn, S-Bahn, Tram, Bus, R-Bahn) with delay info and platform numbers. Supports fuzzy station search with interactive selection.

```bash
dotnet run --project samples/DotVgn.Samples.Monitor -- <StationName>
```

<img src="https://i.imgur.com/3gw1q8Q.png" width=500>