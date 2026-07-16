namespace Palworld.RestClient.Models
{
    /// <summary>Contains general information about the Palworld server.</summary>
    /// <param name="Version">The server software version.</param>
    /// <param name="ServerName">The server's display name.</param>
    /// <param name="Description">The server's description.</param>
    /// <param name="WorldGuid">The unique identifier for the server's world.</param>
    public sealed record ServerInfo(
        string Version,
        string ServerName,
        string Description,
        string WorldGuid
    );
}
