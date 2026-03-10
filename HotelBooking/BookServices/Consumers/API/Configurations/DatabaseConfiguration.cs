using Data.MySql;
using Microsoft.EntityFrameworkCore;

namespace API.Configurations;

public static class DatabaseConfiguration
{
    public static IServiceCollection AddDatabaseConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("MySQLConnection")
            ?? throw new InvalidOperationException("Connection string 'MySQLConnection' não encontrada.");

        services.AddDbContext<HotelDbContext>(options =>
            options.UseMySql(
                connectionString,
                ServerVersion.AutoDetect(connectionString)
            ));

        return services;
    }
}