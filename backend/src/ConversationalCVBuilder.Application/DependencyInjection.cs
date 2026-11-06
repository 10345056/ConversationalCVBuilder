using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using ConversationalCVBuilder.Application.Abstractions;
using ConversationalCVBuilder.Application.Common.Behaviors;
using ConversationalCVBuilder.Application.Mapping;
using ConversationalCVBuilder.Application.Services;
using MediatR;

namespace ConversationalCVBuilder.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();
        services.AddMediatR(configuration => configuration.RegisterServicesFromAssembly(assembly));
        services.AddValidatorsFromAssembly(assembly);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddSingleton<ICvProfileMapper, CvProfileMapper>();
        services.AddSingleton<ITemplateCatalog, BuiltInTemplateCatalog>();
        return services;
    }
}
