namespace CarRent.Application.Common.Settings;

public class GitHubSettings
{
    public const string SectionName = "GitHubSettings";

    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string RedirectUri { get; set; } = "http://localhost:3000/auth/github/callback";
}
