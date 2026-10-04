namespace CarRent.Application.DTOs.Payments;

public class CreatePaymentResponse
{
    public Guid BookingId { get; set; }
    public string OrderId { get; set; } = string.Empty;
    public string ApprovalUrl { get; set; } = string.Empty;
}