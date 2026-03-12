using API.Controllers;
using Application;
using Application.Book.Port;
using Application.Payment.DTO;
using Application.Payment.Requests;
using Application.Payment.Responses;
using Domain.Payment.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace API.Tests.Controllers;

public class PaymentControllerTests
{
    private readonly Mock<ILogger<PaymentController>> _loggerMock;
    private readonly Mock<IBookManager> _bookManagerMock;
    private readonly PaymentController _controller;

    public PaymentControllerTests()
    {
        _loggerMock = new Mock<ILogger<PaymentController>>();
        _bookManagerMock = new Mock<IBookManager>();

        _controller = new PaymentController(
            _loggerMock.Object,
            _bookManagerMock.Object);
    }

    private static PaymentRequest CreatePaymentRequest()
    {
        return new PaymentRequest
        {
            BookingId = 0,
            PaymentIntention = "Payment for booking",
            SelectPaymentProvider = SupportedPaymentProviders.Stripe,
            SelectPaymentMethod = SupportedPaymentMethods.CreditCard
        };
    }

    [Fact]
    public async Task Pay_Should_Return_Ok_When_Payment_Is_Successful()
    {
        var bookingId = 10;
        var request = CreatePaymentRequest();

        var paymentState = new PaymentStateDTO
        {
            State = PaymentState.Success,
            PaymentId = 123,
            CreatedAt = DateTime.UtcNow,
            Message = "Payment processed successfully"
        };

        var response = new PaymentResponse
        {
            Success = true,
            Data = paymentState
        };

        _bookManagerMock
            .Setup(x => x.PayForAbooking(It.Is<PaymentRequest>(r =>
                r.BookingId == bookingId &&
                r.PaymentIntention == request.PaymentIntention &&
                r.SelectPaymentProvider == request.SelectPaymentProvider &&
                r.SelectPaymentMethod == request.SelectPaymentMethod)))
            .ReturnsAsync(response);

        var result = await _controller.Pay(bookingId, request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedData = Assert.IsType<PaymentStateDTO>(okResult.Value);

        Assert.Equal(paymentState.PaymentId, returnedData.PaymentId);
        Assert.Equal(paymentState.State, returnedData.State);
        Assert.Equal(paymentState.Message, returnedData.Message);

        _bookManagerMock.Verify(x => x.PayForAbooking(It.Is<PaymentRequest>(r =>
            r.BookingId == bookingId &&
            r.PaymentIntention == request.PaymentIntention &&
            r.SelectPaymentProvider == request.SelectPaymentProvider &&
            r.SelectPaymentMethod == request.SelectPaymentMethod)), Times.Once);
    }

    [Fact]
    public async Task Pay_Should_Set_BookingId_From_Route_Into_Request()
    {
        var bookingId = 25;
        var request = CreatePaymentRequest();

        _bookManagerMock
            .Setup(x => x.PayForAbooking(It.IsAny<PaymentRequest>()))
            .ReturnsAsync(new PaymentResponse
            {
                Success = true,
                Data = new PaymentStateDTO
                {
                    State = PaymentState.Success,
                    PaymentId = 1,
                    Message = "ok"
                }
            });

        await _controller.Pay(bookingId, request);

        Assert.Equal(bookingId, request.BookingId);
    }

    [Fact]
    public async Task Pay_Should_Return_BadRequest_When_Payment_Fails()
    {
        var bookingId = 10;
        var request = CreatePaymentRequest();

        var response = new PaymentResponse
        {
            Success = false,
            ErrorCode = ErrorCodes.INVALID_PAYMENT,
            Message = "Invalid payment",
            Data = new PaymentStateDTO
            {
                State = PaymentState.Failed,
                PaymentId = 0,
                Message = "Payment failed"
            }
        };

        _bookManagerMock
            .Setup(x => x.PayForAbooking(It.Is<PaymentRequest>(r =>
                r.BookingId == bookingId &&
                r.PaymentIntention == request.PaymentIntention &&
                r.SelectPaymentProvider == request.SelectPaymentProvider &&
                r.SelectPaymentMethod == request.SelectPaymentMethod)))
            .ReturnsAsync(response);

        var result = await _controller.Pay(bookingId, request);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var returnedResponse = Assert.IsType<PaymentResponse>(badRequestResult.Value);

        Assert.False(returnedResponse.Success);
        Assert.Equal(response.ErrorCode, returnedResponse.ErrorCode);
        Assert.Equal(response.Message, returnedResponse.Message);
        Assert.NotNull(returnedResponse.Data);
        Assert.Equal(PaymentState.Failed, returnedResponse.Data.State);
    }

    [Fact]
    public async Task Pay_Should_Return_InternalServerError_When_Exception_Is_Thrown()
    {
        var bookingId = 10;
        var request = CreatePaymentRequest();

        _bookManagerMock
            .Setup(x => x.PayForAbooking(It.IsAny<PaymentRequest>()))
            .ThrowsAsync(new Exception("unexpected error"));

        var result = await _controller.Pay(bookingId, request);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        Assert.Equal("An error occurred while processing the payment.", objectResult.Value);
    }
}