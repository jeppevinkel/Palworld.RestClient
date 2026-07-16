using System.Text.Json.Serialization;

namespace Palworld.RestClient.Internal.Requests
{
    internal sealed record ShutdownRequest(
        [property: JsonPropertyName("waittime")] int WaitTime,
        [property: JsonPropertyName("message")] string Message
    );
}