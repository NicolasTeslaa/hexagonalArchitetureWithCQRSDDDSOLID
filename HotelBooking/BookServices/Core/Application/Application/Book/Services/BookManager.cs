using Application.Book.DTO;
using Application.Book.Port;
using Application.Book.Request;
using Application.Book.Responses;
using Application.Payment.Factory;
using Application.Payment.Ports;
using Application.Payment.Requests;
using Application.Payment.Responses;
using Domain.Book.Exceptions;
using Domain.Book.Ports;

namespace Application.Book.Services;

public class BookManager : IBookManager
{
    private readonly IBookRepository _bookRepository;
    private readonly IPaymentProcessorFactory _paymentProcessorFactory;

    public BookManager(IBookRepository bookRepository, IPaymentProcessorFactory paymentProcessorFactory)
    {
        _bookRepository = bookRepository;
        _paymentProcessorFactory = paymentProcessorFactory;
    }

    public async Task<PaymentResponse> PayForAbooking(PaymentRequest request)
    {
        IPaymentProcessor paymentProcessor = _paymentProcessorFactory.GetPaymentProcessor(request.SelectPaymentProvider);

        var response = await paymentProcessor.CapturePayment(request.PaymentIntention);

        if (response.Success)
        {
            Domain.Book.Entities.Booking? booking = await _bookRepository.GetBookByIdAsync(request.BookingId);

            if (booking is null)
            {
                return new PaymentResponse
                {
                    Success = false,
                    ErrorCode = response.ErrorCode,
                    Message = response.Message
                };
            }

            booking.ChangeState(Domain.Book.Enums.Action.Pay);

            await booking.Save(_bookRepository);

            return new PaymentResponse
            {
                Success = true,
                Data = response.Data
            };
        }

        return new PaymentResponse
        {
            Success = false,
            ErrorCode = response.ErrorCode,
            Message = response.Message
        };
    }
}
