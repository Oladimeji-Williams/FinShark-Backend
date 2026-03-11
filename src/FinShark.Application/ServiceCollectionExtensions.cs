using FinShark.Application.Dtos;
using FinShark.Application.Mappers;
using FinShark.Application.Stocks.Validators;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace FinShark.Application;

/// <summary>
/// Application layer dependency injection extensions
/// Registers application services including MediatR, validators, and mappers
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds application services to the dependency injection container
    /// </summary>
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services)
    {
        // MediatR registration - automatically registers all handlers from this assembly
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssemblyContaining<CreateStockValidator>();
        });

        // FluentValidation - manually register validators
        services.AddScoped<IValidator<CreateStockRequestDto>, CreateStockValidator>();
        services.AddScoped<IValidator<UpdateStockRequestDto>, UpdateStockValidator>();

        // Mappers - registered as singletons since they're stateless
        services.AddSingleton<StockMapper>();

        return services;
    }
}
