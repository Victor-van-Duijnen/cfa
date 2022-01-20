using Core.Domain.Enumerations;
using Core.Domain.Interfaces;
using Core.SeedWork;
using Core.Services;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Database;
using Infrastructure.Persistence.Read;
using Infrastructure.Persistence.Write;
using Infrastructure.Services;
using Infrastructure.Services.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddInfrastructure(this IServiceCollection services, string trainingConnectionString, string userAccountConnectionString,
        bool useConsoleLogger, IConfigurationSection mailOptionSection)
    {
        services.AddContext(trainingConnectionString, useConsoleLogger)
            .AddEventDispatcher()
            .AddRepositories()
            .AddServices(userAccountConnectionString, mailOptionSection)
            .AddQueries(trainingConnectionString);
    }

    private static IServiceCollection AddEventDispatcher(this IServiceCollection services)
    {
        services.AddScoped<IBus, Bus>();
        services.AddScoped<MessageBus>();
        services.AddScoped<EventDispatcher>();
        return services;
    }

    private static IServiceCollection AddContext(this IServiceCollection services, string connectionString,
        bool useConsoleLogger)
    {
        services.AddScoped(provider => new Context(connectionString, useConsoleLogger, provider.GetRequiredService<EventDispatcher>()));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        return services;
    }

    private static IServiceCollection AddServices(this IServiceCollection services, string userAccountConnectionString,
        IConfigurationSection mailOptionSection)
    {
        services.Configure<MailOptions>(mailOptionSection);
        services.AddScoped<IUserStrategy>(provider =>
        {
            var context = provider.GetRequiredService<IHttpContextAccessor>();
            var appName = context.HttpContext?.Request.Headers["application_name"];

            if (string.Equals(appName, ApplicationType.Account.Name))
                return new AccountUserStrategy(userAccountConnectionString);
            return new AccountUserStrategy(userAccountConnectionString);
        });
        services.AddScoped<IMailService, MailService>();

        return services;
    }

    private static IServiceCollection AddQueries(this IServiceCollection services, string connectionString)
    {
        services.AddScoped<ITrainerQueries>(_ => new TrainerQueries(connectionString));
        services.AddScoped<ITrainingQueries>(_ => new TrainingQueries(connectionString));

        return services;
    }

    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<ITrainingRepository, TrainingRepository>();
        services.AddScoped<ITrainerRepository, TrainerRepository>();

        return services;
    }
}
