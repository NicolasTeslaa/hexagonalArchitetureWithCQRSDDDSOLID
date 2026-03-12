using Application.Payment.Ports;
using Application.Payment.Requests;
using Application.Payment.Responses;
using Domain.Payment.Enums;

namespace Application.Payment.Factory;

public interface IPaymentProcessorFactory
{
    IPaymentProcessor GetPaymentProcessor(SupportedPaymentProviders selectedProvider);
}
