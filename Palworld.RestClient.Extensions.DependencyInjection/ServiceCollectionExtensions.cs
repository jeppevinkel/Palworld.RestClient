using System;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace Palworld.RestClient.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
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