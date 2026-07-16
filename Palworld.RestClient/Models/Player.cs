using System.Text.Json.Serialization;

namespace Palworld.RestClient.Models
{
    /// <summary>Represents a player currently connected to the Palworld server.</summary>
    /// <param name="Name">The player's in-game display name.</param>
    /// <param name="AccountName">The player's platform account name.</param>
    /// <param name="PlayerId">The unique player ID.</param>
    /// <param name="UserId">The player's platform-specific user ID (e.g. <c>steam_76561198000000000</c>).</param>
    /// <param name="IP">The player's IP address.</param>
    /// <param name="Ping">The player's current ping in milliseconds.</param>
    /// <param name="LocationX">The player's current X coordinate in the world.</param>
    /// <param name="LocationY">The player's current Y coordinate in the world.</param>
    /// <param name="Level">The player's current in-game level.</param>
    /// <param name="BuildingCount">
    /// The number of buildings owned by the player.
    /// May be <c>null</c> if not provided by the server.
    /// </param>
    public sealed record Player(
        string Name,
        string AccountName,
        string PlayerId,
        string UserId,
        string IP,
        double Ping,
        [property: JsonPropertyName("location_x")] double LocationX,
        [property: JsonPropertyName("location_y")] double LocationY,
        int Level,
        // This property was not returned in my tests, but the spec mentions it should exist.
        [property: JsonPropertyName("building_count")] int? BuildingCount
    );
}
