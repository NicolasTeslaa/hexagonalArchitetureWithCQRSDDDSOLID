using Application.Payment.DTO;

namespace Application.Payment.Services;

public interface IPaymentService
{
    Task<PaymentStateDTO> PayWithCreditCard(string paymentIntention);
    Task<PaymentStateDTO> PayWithDebitCard(string paymentIntention);
    Task<PaymentStateDTO> PayBankTransfer(string paymentIntention);
}
