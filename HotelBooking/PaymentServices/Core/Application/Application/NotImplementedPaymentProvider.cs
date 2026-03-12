using Application;
using Application.Payment.Ports;
using Application.Payment.Requests;
using Application.Payment.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Payment.Application
{
    public class NotImplementedPaymentProvider : IPaymentProcessor
    {
        public async Task<PaymentResponse> CapturePayment(string paymentIntention)
        {
            var paymentResponse = new PaymentResponse()
            {
                Success = false,
                ErrorCode = ErrorCodes.PAYMENT_PROVIDER_NOT_IMPLEMENTED,
                Message = "The selected payment provider is not available at the moment"
            };

            return paymentResponse;
        }
    }
}