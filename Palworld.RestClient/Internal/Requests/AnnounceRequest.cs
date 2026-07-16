using System.Text.Json.Serialization;

namespace Palworld.RestClient.Internal.Requests
{
    internal sealed record AnnounceRequest(
        [property: JsonPropertyName("message")] string Message
    );
}