using Domain.Payment.Enums;

namespace Application.Payment.Requests;

public class PaymentRequest
{
    public int BookingId { get; set; }
    public string PaymentIntention { get; set; }
    public SupportedPaymentProviders SelectPaymentProvider { get; set; }
    public SupportedPaymentMethods SelectPaymentMethod { get; set; }
}
