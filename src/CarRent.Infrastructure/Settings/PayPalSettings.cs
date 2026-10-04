namespace CarRent.Application.Common.Settings;

public class PayPalSettings
{
    public const string SectionName = "PayPalSettings";

    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api-m.sandbox.paypal.com";
    public string Currency { get; set; } = "USD";
    public string BrandName { get; set; } = "CarRent";
    public string ReturnUrl { get; set; } = string.Empty;
    public string CancelUrl { get; set; } = string.Empty;
    public string WebhookId { get; set; } = string.Empty;
}