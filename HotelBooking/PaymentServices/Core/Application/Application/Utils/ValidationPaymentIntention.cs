using Application.Exceptions;

namespace Application.Utils;

public class ValidationPaymentIntention
{
    public static void Validate(string paymentIntention)
    {
        if (string.IsNullOrEmpty(paymentIntention) || paymentIntention.Length > 255)
            throw new InvalidPaymentIntentionException();
    }
}
