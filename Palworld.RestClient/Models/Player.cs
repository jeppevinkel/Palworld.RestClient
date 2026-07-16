using System.Text.Json.Serialization;

namespace Palworld.RestClient.Models
{
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