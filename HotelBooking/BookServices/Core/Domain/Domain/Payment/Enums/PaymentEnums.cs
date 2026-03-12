namespace Domain.Payment.Enums;

public enum SupportedPaymentProviders
{
    MercadoPago = 0,
    Paypal = 1,
    Stripe = 2,
    PagSeguro = 3,
}

public enum SupportedPaymentMethods
{
    CreditCard = 0,
    DebitCard = 1,
    BankTransfer = 2,
    DigitalWallet = 3,
}