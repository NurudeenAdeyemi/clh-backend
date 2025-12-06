using CLHCRM.Application.Interfaces;
using CLHCRM.Infrastructure.Persistence;
using CLHCRM.Infrastructure.Persistence.Interceptors;
using CLHCRM.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CLHCRM.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Services
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // Interceptors
        services.AddSingleton<AuditableEntityInterceptor>();

        // Database
        services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
        {
            var interceptor = serviceProvider.GetRequiredService<AuditableEntityInterceptor>();

            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName))
                .AddInterceptors(interceptor);
        });

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Repositories will be registered here
        // Example: services.AddScoped<IStudentRepository, StudentRepository>();

        return services;
    }
}
