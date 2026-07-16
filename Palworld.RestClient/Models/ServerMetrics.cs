namespace Palworld.RestClient.Models
{
    /// <summary>Contains real-time performance metrics for the Palworld server.</summary>
    /// <param name="CurrentPlayerNum">The number of players currently connected.</param>
    /// <param name="ServerFps">The current server frames per second.</param>
    /// <param name="ServerFpsAverage">The average server frames per second.</param>
    /// <param name="ServerFrameTime">The current server frame time in milliseconds.</param>
    /// <param name="Days">The number of in-game days elapsed.</param>
    /// <param name="MaxPlayerNum">The maximum number of players the server allows.</param>
    /// <param name="BaseCampNum">The number of base camps currently on the server.</param>
    /// <param name="Uptime">The server uptime in seconds.</param>
    public sealed record ServerMetrics(
        int CurrentPlayerNum,
        int ServerFps,
        double ServerFpsAverage,
        double ServerFrameTime,
        int Days,
        int MaxPlayerNum,
        int BaseCampNum,
        int Uptime
    );
}
