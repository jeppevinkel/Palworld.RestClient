using System;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace Palworld.RestClient.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for registering Palworld REST client services
    /// with the dependency injection container.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers <see cref="PalworldClient"/> as a typed HTTP client
        /// in the dependency injection container.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
        /// <param name="baseAddress">
        /// The base URL of the Palworld server REST API (e.g. <c>http://localhost:8212</c>).
        /// </param>
        /// <param name="password">The admin password configured on the server.</param>
        /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="baseAddress"/> or <paramref name="password"/> is null, empty,
        /// or if <paramref name="baseAddress"/> is not a valid absolute URI.
        /// </exception>
        public static IServiceCollection AddPalworldClient(
            this IServiceCollection services,
            string baseAddress,
            string password)
        {
            if (string.IsNullOrWhiteSpace(baseAddress))
                throw new ArgumentException("Base address is required.", nameof(baseAddress));
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password is required.", nameof(password));
            if (!Uri.TryCreate(baseAddress, UriKind.Absolute, out var uri))
                throw new ArgumentException($"'{baseAddress}' is not a valid absolute URI.", nameof(baseAddress));

            services.AddHttpClient<PalworldClient>(client =>
            {
                client.BaseAddress = uri;

                var credentials = Convert.ToBase64String(
                    Encoding.UTF8.GetBytes($"admin:{password}"));

                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Basic", credentials);
            });

            return services;
        }
    }
}
