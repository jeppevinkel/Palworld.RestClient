namespace Palworld.RestClient.Models
{
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