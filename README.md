# Palworld.RestClient

A .NET client library for the [Palworld](https://store.steampowered.com/app/1623730/Palworld/) dedicated server REST API.

## Packages

| Package | NuGet |
|---|---|
| `Palworld.RestClient` | [![NuGet](https://img.shields.io/nuget/v/Palworld.RestClient)](https://www.nuget.org/packages/Palworld.RestClient) |
| `Palworld.RestClient.Extensions.DependencyInjection` | [![NuGet](https://img.shields.io/nuget/v/Palworld.RestClient.Extensions.DependencyInjection)](https://www.nuget.org/packages/Palworld.RestClient.Extensions.DependencyInjection) |

## Server configuration

To enable the REST API, set the following in your server's `PalWorldSettings.ini`:

```ini
RESTAPIEnabled=True
AdminPassword="your-password"
```

The `AdminPassword` value is what you pass as the `password` parameter to this client.

## Quick start

### Without dependency injection

```csharp
using Palworld.RestClient;

using var client = PalworldClient.Create("http://localhost:8212", "your-password");

var info = await client.GetServerInfoAsync();
Console.WriteLine($"{info?.ServerName} ({info?.Version})");

var players = await client.GetPlayersAsync();
foreach (var player in players ?? [])
    Console.WriteLine($"{player.Name} — Level {player.Level} — {player.Ping}ms");
```

### With dependency injection

```csharp
// Registration
builder.Services.AddPalworldClient(
    baseAddress: "http://localhost:8212",
    password: "your-password");

// Usage
public class MyService(PalworldClient palworld)
{
    public async Task KickCheaterAsync(string userId)
        => await palworld.KickPlayerAsync(userId, "Cheating is not allowed.");
}
```

## Available methods

| Method | Description |
|---|---|
| `GetServerInfoAsync()` | Server version, name, and description |
| `GetMetricsAsync()` | FPS, player count, uptime, and more |
| `GetSettingsAsync()` | Raw server configuration settings |
| `GetPlayersAsync()` | List of currently connected players |
| `KickPlayerAsync(userId, reason)` | Kick a player from the server |
| `BanPlayerAsync(userId, reason)` | Ban a player from the server |
| `UnbanPlayerAsync(userId)` | Remove a player's ban |
| `AnnounceAsync(message)` | Broadcast a message to all players |
| `ShutdownAsync(waitSeconds, message)` | Gracefully shut down the server after a delay |
| `SaveAsync()` | Save the current game state |
| `ForceStopAsync()` | Immediately stop the server process |

## Requirements

- .NET Standard 2.0 or later
