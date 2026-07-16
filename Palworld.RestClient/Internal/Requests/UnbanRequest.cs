using System.Text.Json.Serialization;

namespace Palworld.RestClient.Internal.Requests
{
    internal sealed record UnbanRequest(
        [property: JsonPropertyName("userid")] string UserId
    );
}