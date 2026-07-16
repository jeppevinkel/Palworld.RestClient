using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Palworld.RestClient.Internal.Requests;
using Palworld.RestClient.Models;

namespace Palworld.RestClient
{
    public sealed class PalworldClient : IDisposable
    {
        private const string AdminUsername = "admin";
        private readonly HttpClient _http;
        private readonly bool _ownsHttpClient;

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public PalworldClient(HttpClient http)
        {
            _http = http;
            _ownsHttpClient = false; // DI/caller owns it
        }

        private PalworldClient(HttpClient http, bool ownsHttpClient)
        {
            _http = http;
            _ownsHttpClient = ownsHttpClient;
        }

        public static PalworldClient Create(string baseAddress, string password)
        {
            if (string.IsNullOrWhiteSpace(baseAddress))
                throw new ArgumentException("Base address is required.", nameof(baseAddress));
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password is required.", nameof(password));
            if (!Uri.TryCreate(baseAddress, UriKind.Absolute, out var uri))
                throw new ArgumentException($"'{baseAddress}' is not a valid absolute URI.", nameof(baseAddress));

            var credentials = Convert.ToBase64String(
                Encoding.UTF8.GetBytes($"{AdminUsername}:{password}"));

            var http = new HttpClient
            {
                BaseAddress = uri,
                DefaultRequestHeaders =
                {
                    Authorization = new AuthenticationHeaderValue("Basic", credentials)
                }
            };

            return new PalworldClient(http, ownsHttpClient: true);
        }

        #region GetRequests

        public Task<ServerInfo?> GetServerInfoAsync(CancellationToken ct = default)
            => GetJsonAsync<ServerInfo>("/v1/api/info", ct);

        public Task<ServerMetrics?> GetMetricsAsync(CancellationToken ct = default)
            => GetJsonAsync<ServerMetrics>("/v1/api/metrics", ct);

        public Task<Dictionary<string, JsonElement>?> GetSettingsAsync(CancellationToken ct = default)
            => GetJsonAsync<Dictionary<string, JsonElement>>("/v1/api/settings", ct);

        public async Task<IReadOnlyList<Player>?> GetPlayersAsync(CancellationToken ct = default)
        {
            var result = await GetJsonAsync<PlayerList>("/v1/api/players", ct);
            return result?.Players;
        }

        #endregion

        #region PostRequests

        public async Task KickPlayerAsync(string userId, string reason = "", CancellationToken ct = default)
        {
            var response = await _http.PostAsync(
                "/v1/api/kick",
                CreateJsonContent(new PlayerActionRequest(userId, reason)),
                ct);
            response.EnsureSuccessStatusCode();
        }

        public async Task BanPlayerAsync(string userId, string reason = "", CancellationToken ct = default)
        {
            var response = await _http.PostAsync(
                "/v1/api/ban",
                CreateJsonContent(new PlayerActionRequest(userId, reason)),
                ct);
            response.EnsureSuccessStatusCode();
        }

        public async Task UnbanPlayerAsync(string userId, CancellationToken ct = default)
        {
            var response = await _http.PostAsync(
                "/v1/api/unban",
                CreateJsonContent(new UnbanRequest(userId)),
                ct);
            response.EnsureSuccessStatusCode();
        }

        public async Task AnnounceAsync(string message, CancellationToken ct = default)
        {
            var response = await _http.PostAsync(
                "/v1/api/announce",
                CreateJsonContent(new AnnounceRequest(message)),
                ct);
            response.EnsureSuccessStatusCode();
        }

        public async Task ShutdownAsync(int waitSeconds = 60, string message = "Server shutting down",
            CancellationToken ct = default)
        {
            var response = await _http.PostAsync(
                "/v1/api/shutdown",
                CreateJsonContent(new ShutdownRequest(waitSeconds, message)),
                ct);
            response.EnsureSuccessStatusCode();
        }

        public async Task SaveAsync(CancellationToken ct = default)
        {
            var response = await _http.PostAsync("/v1/api/save", null, ct);
            response.EnsureSuccessStatusCode();
        }

        public async Task ForceStopAsync(CancellationToken ct = default)
        {
            var response = await _http.PostAsync("/v1/api/stop", null, ct);
            response.EnsureSuccessStatusCode();
        }

        #endregion

        #region JsonHelpers

        private async Task<T?> GetJsonAsync<T>(string requestUri, CancellationToken ct)
        {
            var response = await _http.GetAsync(requestUri, ct);
            response.EnsureSuccessStatusCode();
            var stream = await response.Content.ReadAsStreamAsync();
            return await JsonSerializer.DeserializeAsync<T>(stream, JsonOptions, cancellationToken: ct);
        }

        private static ByteArrayContent CreateJsonContent<T>(T value)
        {
            // Serializes directly to UTF-8 bytes — no intermediate string allocation
            var bytes = JsonSerializer.SerializeToUtf8Bytes(value);
            var content = new ByteArrayContent(bytes);
            // ByteArrayContent always knows its length → Content-Length header is always set
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json")
            {
                CharSet = "utf-8"
            };
            return content;
        }

        #endregion

        public void Dispose()
        {
            if (_ownsHttpClient)
                _http.Dispose();
        }
    }
}