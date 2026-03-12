using Application.Book.Commands;
using Application.Book.Port;
using Application.Book.Services;
using Application.Guest.Port;
using Application.Guest.Services;
using Application.MercadoPago.Adapter;
using Application.Payment.Factory;
using Application.Payment.Ports;
using Application.Paypal.Adapter;
using Application.Room.Port;
using Application.Room.Services;
using Application.Stripe.Adapter;
using Data.MySql.Repositories;
using Domain.Book.Ports;
using Domain.Guest.Ports;
using Domain.Room.Ports;
using MediatR;
using Payment.Application.Factory;

namespace API.Configurations;

public static class DependencyInjection
{
    public static IServiceCollection AddProjectDependencies(this IServiceCollection services)
    {
        services.AddScoped<IGuestManager, GuestManager>();
        services.AddScoped<IGuestRepository, GuestRepository>();

        services.AddScoped<IRoomManager, RoomManager>();
        services.AddScoped<IRoomRepository, RoomRepository>();

        services.AddScoped<IBookManager, BookManager>();
        services.AddScoped<IBookRepository, BookRepository>();

        services.AddScoped<IPaymentProcessor, MercadoPagoAdapter>();
        services.AddScoped<IPaymentProcessor, StripeAdapter>();
        services.AddScoped<IPaymentProcessor, PaypalAdapter>();

        services.AddScoped<IPaymentProcessorFactory, PaymentProcessorFactory>();

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(CreateBookingHandler).Assembly));

        return services;
    }
}