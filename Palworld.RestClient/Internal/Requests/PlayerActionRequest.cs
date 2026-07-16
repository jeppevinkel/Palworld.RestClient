using System.Text.Json.Serialization;

namespace Palworld.RestClient.Internal.Requests
{
    internal sealed record PlayerActionRequest(
        [property: JsonPropertyName("userid")] string UserId,
        [property: JsonPropertyName("message")] string Message
    );
}