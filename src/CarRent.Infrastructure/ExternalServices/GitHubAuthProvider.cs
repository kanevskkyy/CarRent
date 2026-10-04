using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using CarRent.Application.Common.Settings;
using CarRent.Application.Exceptions;
using CarRent.Application.Interfaces.Services;
using CarRent.Application.Models.Auth;
using Microsoft.Extensions.Options;

namespace CarRent.Infrastructure.ExternalServices;

public class GitHubAuthProvider : IExternalAuthProvider
{
    private readonly GitHubSettings _settings;
    private readonly IHttpClientFactory _httpClientFactory;

    public GitHubAuthProvider(IOptions<GitHubSettings> options, IHttpClientFactory httpClientFactory)
    {
        _settings = options.Value;
        _httpClientFactory = httpClientFactory;
    }

    public string Provider => "GitHub";

    public async Task<ExternalUserInfo> ValidateAsync(string code, CancellationToken cancellationToken)
    {
        HttpClient client = _httpClientFactory.CreateClient();

        using HttpRequestMessage tokenRequest = new(HttpMethod.Post, "https://github.com/login/oauth/access_token")
        {
            Content = JsonContent.Create(new
            {
                client_id = _settings.ClientId,
                client_secret = _settings.ClientSecret,
                code
            })
        };
        tokenRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using HttpResponseMessage tokenResponse = await client.SendAsync(tokenRequest, cancellationToken);
        using JsonDocument tokenJson = JsonDocument.Parse(
            await tokenResponse.Content.ReadAsStringAsync(cancellationToken));

        if (!tokenJson.RootElement.TryGetProperty("access_token", out JsonElement tokenElement)
            || string.IsNullOrWhiteSpace(tokenElement.GetString()))
            throw new UnauthorizedException("Invalid GitHub code.");

        string accessToken = tokenElement.GetString()!;

        using JsonDocument profile = await GetJsonAsync(client, "https://api.github.com/user", accessToken, cancellationToken);

        long id = profile.RootElement.GetProperty("id").GetInt64();
        string? name = GetString(profile.RootElement, "name") ?? GetString(profile.RootElement, "login");

        string? email = null;
        using JsonDocument emails = await GetJsonAsync(client, "https://api.github.com/user/emails", accessToken, cancellationToken);

        foreach (JsonElement item in emails.RootElement.EnumerateArray())
        {
            bool verified = item.TryGetProperty("verified", out JsonElement v) && v.GetBoolean();
            bool primary = item.TryGetProperty("primary", out JsonElement p) && p.GetBoolean();
            if (verified && primary)
            {
                email = GetString(item, "email");
                break;
            }
        }

        return new ExternalUserInfo(Provider, id.ToString(), email, EmailVerified: email is not null, name);
    }

    private static async Task<JsonDocument> GetJsonAsync(
        HttpClient client, string url, string accessToken, CancellationToken cancellationToken)
    {
        using HttpRequestMessage request = new(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        request.Headers.UserAgent.Add(new ProductInfoHeaderValue("CarRent", "1.0")); // GitHub requires User-Agent

        using HttpResponseMessage response = await client.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new UnauthorizedException("Failed to get GitHub profile.");

        return JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
    }

    private static string? GetString(JsonElement element, string property) =>
        element.TryGetProperty(property, out JsonElement value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
}