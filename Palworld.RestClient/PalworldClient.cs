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
    /// <summary>
    /// A client for interacting with the Palworld dedicated server REST API.
    /// </summary>
    public sealed class PalworldClient : IDisposable
    {
        private const string AdminUsername = "admin";
        private readonly HttpClient _http;
        private readonly bool _ownsHttpClient;

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        /// <summary>
        /// Initializes a new instance of <see cref="PalworldClient"/> with an externally managed <see cref="HttpClient"/>.
        /// Intended for use with dependency injection via <c>IHttpClientFactory</c>.
        /// </summary>
        /// <param name="http">
        /// The <see cref="HttpClient"/> to use for requests.
        /// The caller is responsible for its configuration and lifetime.
        /// </param>
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

        /// <summary>
        /// Creates a self-contained <see cref="PalworldClient"/> that manages its own <see cref="HttpClient"/>.
        /// Dispose the returned instance when done to release the underlying HTTP resources.
        /// </summary>
        /// <param name="baseAddress">
        /// The base URL of the Palworld server REST API (e.g. <c>http://localhost:8212</c>).
        /// </param>
        /// <param name="password">The admin password configured on the server.</param>
        /// <returns>A configured <see cref="PalworldClient"/> instance.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="baseAddress"/> or <paramref name="password"/> is null, empty,
        /// or if <paramref name="baseAddress"/> is not a valid absolute URI.
        /// </exception>
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

        /// <summary>Retrieves general information about the server.</summary>
        /// <param name="ct">A token to cancel the request.</param>
        /// <returns>A <see cref="ServerInfo"/> instance, or <c>null</c> if deserialization fails.</returns>
        public Task<ServerInfo?> GetServerInfoAsync(CancellationToken ct = default)
            => GetJsonAsync<ServerInfo>("/v1/api/info", ct);

        /// <summary>Retrieves real-time performance metrics for the server.</summary>
        /// <param name="ct">A token to cancel the request.</param>
        /// <returns>A <see cref="ServerMetrics"/> instance, or <c>null</c> if deserialization fails.</returns>
        public Task<ServerMetrics?> GetMetricsAsync(CancellationToken ct = default)
            => GetJsonAsync<ServerMetrics>("/v1/api/metrics", ct);

        /// <summary>
        /// Retrieves the server's current configuration settings as a raw key-value collection.
        /// </summary>
        /// <param name="ct">A token to cancel the request.</param>
        /// <returns>A dictionary of setting names to their JSON values, or <c>null</c> if deserialization fails.</returns>
        public Task<Dictionary<string, JsonElement>?> GetSettingsAsync(CancellationToken ct = default)
            => GetJsonAsync<Dictionary<string, JsonElement>>("/v1/api/settings", ct);

        /// <summary>Retrieves the list of currently connected players.</summary>
        /// <param name="ct">A token to cancel the request.</param>
        /// <returns>A read-only list of <see cref="Player"/> instances, or <c>null</c> if deserialization fails.</returns>
        public async Task<IReadOnlyList<Player>?> GetPlayersAsync(CancellationToken ct = default)
        {
            var result = await GetJsonAsync<PlayerList>("/v1/api/players", ct);
            return result?.Players;
        }

        #endregion

        #region PostRequests

        /// <summary>Kicks a player from the server.</summary>
        /// <param name="userId">The user ID of the player to kick (e.g. <c>steam_76561198000000000</c>).</param>
        /// <param name="reason">An optional message displayed to the player upon being kicked.</param>
        /// <param name="ct">A token to cancel the request.</param>
        public async Task KickPlayerAsync(string userId, string reason = "", CancellationToken ct = default)
        {
            var response = await _http.PostAsync(
                "/v1/api/kick",
                CreateJsonContent(new PlayerActionRequest(userId, reason)),
                ct);
            response.EnsureSuccessStatusCode();
        }

        /// <summary>Permanently bans a player from the server.</summary>
        /// <param name="userId">The user ID of the player to ban (e.g. <c>steam_76561198000000000</c>).</param>
        /// <param name="reason">An optional message displayed to the player upon being banned.</param>
        /// <param name="ct">A token to cancel the request.</param>
        public async Task BanPlayerAsync(string userId, string reason = "", CancellationToken ct = default)
        {
            var response = await _http.PostAsync(
                "/v1/api/ban",
                CreateJsonContent(new PlayerActionRequest(userId, reason)),
                ct);
            response.EnsureSuccessStatusCode();
        }

        /// <summary>Removes an existing ban for a player.</summary>
        /// <param name="userId">The user ID of the player to unban (e.g. <c>steam_76561198000000000</c>).</param>
        /// <param name="ct">A token to cancel the request.</param>
        public async Task UnbanPlayerAsync(string userId, CancellationToken ct = default)
        {
            var response = await _http.PostAsync(
                "/v1/api/unban",
                CreateJsonContent(new UnbanRequest(userId)),
                ct);
            response.EnsureSuccessStatusCode();
        }

        /// <summary>Broadcasts an announcement message to all connected players.</summary>
        /// <param name="message">The message to broadcast.</param>
        /// <param name="ct">A token to cancel the request.</param>
        public async Task AnnounceAsync(string message, CancellationToken ct = default)
        {
            var response = await _http.PostAsync(
                "/v1/api/announce",
                CreateJsonContent(new AnnounceRequest(message)),
                ct);
            response.EnsureSuccessStatusCode();
        }

        /// <summary>Initiates a graceful server shutdown after a configurable delay.</summary>
        /// <param name="waitSeconds">Seconds to wait before shutting down. Defaults to <c>60</c>.</param>
        /// <param name="message">A message displayed to players before the shutdown.</param>
        /// <param name="ct">A token to cancel the request.</param>
        public async Task ShutdownAsync(int waitSeconds = 60, string message = "Server shutting down",
            CancellationToken ct = default)
        {
            var response = await _http.PostAsync(
                "/v1/api/shutdown",
                CreateJsonContent(new ShutdownRequest(waitSeconds, message)),
                ct);
            response.EnsureSuccessStatusCode();
        }

        /// <summary>Saves the current game state to disk.</summary>
        /// <param name="ct">A token to cancel the request.</param>
        public async Task SaveAsync(CancellationToken ct = default)
        {
            var response = await _http.PostAsync("/v1/api/save", null, ct);
            response.EnsureSuccessStatusCode();
        }

        /// <summary>Immediately stops the server process without a graceful shutdown.</summary>
        /// <param name="ct">A token to cancel the request.</param>
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

        /// <summary>
        /// Releases the <see cref="HttpClient"/> if it was created internally by <see cref="Create"/>.
        /// Has no effect if the client was provided externally via the constructor.
        /// </summary>
        public void Dispose()
        {
            if (_ownsHttpClient)
                _http.Dispose();
        }
    }
}
