# Palworld.RestClient.Extensions.DependencyInjection

`Microsoft.Extensions.DependencyInjection` integration for [Palworld.RestClient](https://www.nuget.org/packages/Palworld.RestClient).

## Installation

```bash
dotnet add package Palworld.RestClient
dotnet add package Palworld.RestClient.Extensions.DependencyInjection
```

## Usage

Register the client during app startup:

```csharp
builder.Services.AddPalworldClient(
    baseAddress: "http://localhost:8212",
    password: "your-password");
```

Inject `PalworldClient` wherever you need it:

```csharp
public class ServerMonitor(PalworldClient palworld)
{
    public async Task<string> GetStatusAsync()
    {
        var metrics = await palworld.GetMetricsAsync();
        return $"{metrics?.CurrentPlayerNum}/{metrics?.MaxPlayerNum} players online";
    }
}
```

## Notes

- Uses `IHttpClientFactory` for proper `HttpClient` lifetime management.
- For use without dependency injection, use `PalworldClient.Create()` from the main package directly.

## Server configuration

To enable the REST API, set the following in your server's `PalWorldSettings.ini`:

```ini
RESTAPIEnabled=True
AdminPassword="your-password"
```

The `AdminPassword` value is what you pass as the `password` parameter to `AddPalworldClient`.
