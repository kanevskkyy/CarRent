using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using CarRent.Application.Common.Settings;
using CarRent.Application.Exceptions;
using CarRent.Application.Interfaces.Services;
using CarRent.Application.Models.Payments;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CarRent.Infrastructure.ExternalServices;

public class PayPalPaymentGateway : IPaymentGateway
{
    private static string? _accessToken;
    private static DateTime _tokenExpiresAt = DateTime.MinValue;
    private static readonly SemaphoreSlim TokenLock = new(1, 1);

    private readonly HttpClient _http;
    private readonly PayPalSettings _settings;
    private readonly ILogger<PayPalPaymentGateway> _logger;

    public PayPalPaymentGateway(
        HttpClient http,
        IOptions<PayPalSettings> options,
        ILogger<PayPalPaymentGateway> logger)
    {
        _settings = options.Value;
        _logger = logger;
        _http = http;
        _http.BaseAddress = new Uri(_settings.BaseUrl.TrimEnd('/') + "/");
    }

    public async Task<PaymentOrder> CreateOrderAsync(
        decimal amount, string description, string referenceId, CancellationToken cancellationToken)
    {
        string payload = JsonSerializer.Serialize(new
        {
            intent = "CAPTURE",
            purchase_units = new[]
            {
                new
                {
                    reference_id = referenceId,
                    custom_id = referenceId,
                    description,
                    amount = new
                    {
                        currency_code = _settings.Currency,
                        value = amount.ToString("F2", CultureInfo.InvariantCulture)
                    }
                }
            },
            payment_source = new
            {
                paypal = new
                {
                    experience_context = new
                    {
                        return_url = _settings.ReturnUrl,
                        cancel_url = _settings.CancelUrl,
                        brand_name = _settings.BrandName,
                        user_action = "PAY_NOW"
                    }
                }
            }
        });

        (HttpStatusCode status, string body) = await SendAsync(
            HttpMethod.Post, "v2/checkout/orders", payload, Guid.NewGuid().ToString(), cancellationToken);

        if ((int)status is < 200 or >= 300)
        {
            _logger.LogError("PayPal create order failed: {Status} {Body}", status, body);
            throw new ValidationException("Could not create PayPal order.");
        }

        using JsonDocument doc = JsonDocument.Parse(body);
        string orderId = doc.RootElement.GetProperty("id").GetString()!;

        string? approvalUrl = null;
        foreach (JsonElement link in doc.RootElement.GetProperty("links").EnumerateArray())
        {
            string? rel = link.GetProperty("rel").GetString();
            if (rel is "payer-action" or "approve")
            {
                approvalUrl = link.GetProperty("href").GetString();
                break;
            }
        }

        if (approvalUrl is null)
        {
            _logger.LogError("PayPal order {OrderId} has no approval link: {Body}", orderId, body);
            throw new ValidationException("Could not create PayPal order.");
        }

        return new PaymentOrder(orderId, approvalUrl);
    }

    public async Task CaptureOrderAsync(string orderId, CancellationToken cancellationToken)
    {
        (HttpStatusCode status, string body) = await SendAsync(
            HttpMethod.Post, $"v2/checkout/orders/{orderId}/capture", "{}", $"capture-{orderId}", cancellationToken);

        if ((int)status is < 200 or >= 300)
        {
            if (body.Contains("ORDER_ALREADY_CAPTURED", StringComparison.Ordinal))
                return;
            
            _logger.LogError("PayPal capture failed for {OrderId}: {Status} {Body}", orderId, status, body);
            throw new ValidationException("Payment could not be captured. Make sure it was approved in PayPal.");
        }

        using JsonDocument doc = JsonDocument.Parse(body);
        string? orderStatus = doc.RootElement.GetProperty("status").GetString();

        if (orderStatus != "COMPLETED")
        {
            _logger.LogWarning("PayPal order {OrderId} captured with status {Status}", orderId, orderStatus);
            throw new ValidationException($"Payment is not completed (status: {orderStatus}).");
        }
    }

    public async Task RefundOrderAsync(string orderId, CancellationToken cancellationToken)
    {
        (HttpStatusCode orderStatus, string orderBody) = await SendAsync(
            HttpMethod.Get, $"v2/checkout/orders/{orderId}", null, null, cancellationToken);

        if (orderStatus != HttpStatusCode.OK)
        {
            _logger.LogError("PayPal get order failed for {OrderId}: {Status} {Body}", orderId, orderStatus, orderBody);
            throw new ValidationException("Could not load the payment for refund.");
        }

        string? captureId;
        using (JsonDocument orderDoc = JsonDocument.Parse(orderBody))
        {
            captureId = orderDoc.RootElement
                .GetProperty("purchase_units")[0]
                .GetProperty("payments")
                .GetProperty("captures")[0]
                .GetProperty("id")
                .GetString();
        }

        if (string.IsNullOrWhiteSpace(captureId))
            throw new ValidationException("Payment has no capture to refund.");

        (HttpStatusCode status, string body) = await SendAsync(
            HttpMethod.Post, $"v2/payments/captures/{captureId}/refund", "{}", $"refund-{captureId}", cancellationToken);

        if ((int)status is >= 200 and < 300)
            return;

        if (body.Contains("CAPTURE_FULLY_REFUNDED", StringComparison.Ordinal))
            return;

        _logger.LogError("PayPal refund failed for capture {CaptureId}: {Status} {Body}", captureId, status, body);
        throw new ValidationException("Refund failed. Please try again later.");
    }

    private async Task<(HttpStatusCode Status, string Body)> SendAsync(
        HttpMethod method, string url, string? jsonBody, string? requestId, CancellationToken cancellationToken)
    {
        string token = await GetAccessTokenAsync(cancellationToken);

        using HttpRequestMessage request = new(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        if (requestId is not null)
            request.Headers.Add("PayPal-Request-Id", requestId);

        if (jsonBody is not null)
            request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

        using HttpResponseMessage response = await _http.SendAsync(request, cancellationToken);
        string body = await response.Content.ReadAsStringAsync(cancellationToken);

        return (response.StatusCode, body);
    }

    private async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        if (_accessToken is not null && DateTime.UtcNow < _tokenExpiresAt)
            return _accessToken;

        await TokenLock.WaitAsync(cancellationToken);
        try
        {
            if (_accessToken is not null && DateTime.UtcNow < _tokenExpiresAt)
                return _accessToken;

            using HttpRequestMessage request = new(HttpMethod.Post, "v1/oauth2/token");
            string basic = Convert.ToBase64String(
                Encoding.UTF8.GetBytes($"{_settings.ClientId}:{_settings.ClientSecret}"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", basic);
            request.Content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials")
            });

            using HttpResponseMessage response = await _http.SendAsync(request, cancellationToken);
            string body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("PayPal authentication failed: {Status} {Body}", response.StatusCode, body);
                throw new InvalidOperationException("PayPal authentication failed. Check ClientId/ClientSecret.");
            }

            using JsonDocument doc = JsonDocument.Parse(body);
            _accessToken = doc.RootElement.GetProperty("access_token").GetString()!;
            int expiresIn = doc.RootElement.GetProperty("expires_in").GetInt32();
            _tokenExpiresAt = DateTime.UtcNow.AddSeconds(expiresIn - 60);

            return _accessToken;
        }
        finally
        {
            TokenLock.Release();
        }
    }
    
    public async Task<bool> VerifyWebhookAsync(
        WebhookHeaders headers, string rawBody, CancellationToken cancellationToken)
    {
        JsonNode? webhookEvent = JsonNode.Parse(rawBody);

        string payload = JsonSerializer.Serialize(new
        {
            auth_algo = headers.AuthAlgo,
            cert_url = headers.CertUrl,
            transmission_id = headers.TransmissionId,
            transmission_sig = headers.TransmissionSig,
            transmission_time = headers.TransmissionTime,
            webhook_id = _settings.WebhookId,
            webhook_event = webhookEvent
        });

        (HttpStatusCode status, string body) = await SendAsync(
            HttpMethod.Post, "v1/notifications/verify-webhook-signature", payload, null, cancellationToken);

        if (status != HttpStatusCode.OK)
        {
            _logger.LogError("PayPal webhook verification call failed: {Status} {Body}", status, body);
            return false;
        }

        using JsonDocument doc = JsonDocument.Parse(body);
        return doc.RootElement.GetProperty("verification_status").GetString() == "SUCCESS";
    }
}