using Application.Book.Port;
using Application.Book.Services;
using Application.Guest.Port;
using Application.Guest.Services;
using Application.Room.Port;
using Application.Room.Services;
using Data.MySql.Repositories;
using Domain.Book.Ports;
using Domain.Guest.Ports;
using Domain.Room.Ports;

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

        return services;
    }
}