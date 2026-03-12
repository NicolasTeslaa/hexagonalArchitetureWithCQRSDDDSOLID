using Application.MercadoPago.Adapter;
using Application.Payment.Factory;
using Application.Payment.Ports;
using Application.Paypal.Adapter;
using Application.Stripe.Adapter;
using Domain.Payment.Enums;

namespace Payment.Application.Factory;

public class PaymentProcessorFactory : IPaymentProcessorFactory
{
    public IPaymentProcessor GetPaymentProcessor(SupportedPaymentProviders selectedPaymentProvider)
    {
        switch (selectedPaymentProvider)
        {
            case SupportedPaymentProviders.MercadoPago:
                return new MercadoPagoAdapter();
            case SupportedPaymentProviders.Paypal:
                return new PaypalAdapter();
            case SupportedPaymentProviders.Stripe:
                return new StripeAdapter();
            default: return new NotImplementedPaymentProvider();
        }
    }
}
