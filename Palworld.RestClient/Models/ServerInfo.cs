namespace Palworld.RestClient.Models
{
    public sealed record ServerInfo(
        string Version,
        string ServerName,
        string Description,
        string WorldGuid
    );
}