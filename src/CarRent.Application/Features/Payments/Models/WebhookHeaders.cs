namespace CarRent.Application.Models.Payments;

public record WebhookHeaders(
    string AuthAlgo,
    string CertUrl,
    string TransmissionId,
    string TransmissionSig,
    string TransmissionTime);