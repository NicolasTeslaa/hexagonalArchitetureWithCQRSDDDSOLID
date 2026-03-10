using Application.Guest.Port;
using Application.Guest.Services;
using Application.Room.Port;
using Application.Room.Services;
using Data.MySql.Repositories;
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

        return services;
    }
}