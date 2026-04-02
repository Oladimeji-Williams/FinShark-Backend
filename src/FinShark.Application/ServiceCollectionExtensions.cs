using FinShark.Application.Auth.Commands.AssignRole;
using FinShark.Application.Auth.Commands.ChangePassword;
using FinShark.Application.Auth.Commands.Login;
using FinShark.Application.Auth.Commands.Register;
using FinShark.Application.Auth.Commands.UpdateProfile;
using FinShark.Application.Auth.Validators;
using FinShark.Application.Comments.Commands.CreateComment;
using FinShark.Application.Comments.Commands.UpdateComment;
using FinShark.Application.Comments.Queries.GetAllComments;
using FinShark.Application.Comments.Queries.GetCommentsByStockId;
using FinShark.Application.Comments.Validators;
using FinShark.Application.Services;
using FinShark.Application.Stocks.Commands.CreateStock;
using FinShark.Application.Stocks.Commands.UpdateStock;
using FinShark.Application.Stocks.Queries.GetStocks;
using FinShark.Application.Stocks.Validators;
using FinShark.Application.Behaviors;
using FluentValidation;
using MediatorFlow.Core.Abstractions;
using MediatorFlow.Core.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace FinShark.Application;

/// <summary>
/// Application layer dependency injection extensions
/// Registers application services including MediatorFlow, validators, and pipeline behaviors
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds application services to the dependency injection container
    /// </summary>
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services)
    {
        // MediatorFlow registration - automatically registers all handlers from this assembly
        services.AddMediatorFlow(typeof(CreateStockValidator).Assembly);

        // Register custom dispatcher for MediatorFlow.Core
        services.AddSingleton<IDispatcher, Dispatcher>();

        // FluentValidation - manually register command validators
        services.AddScoped<IValidator<CreateStockCommand>, CreateStockValidator>();
        services.AddScoped<IValidator<UpdateStockCommand>, UpdateStockValidator>();
        services.AddScoped<IValidator<GetStocksQuery>, GetStocksQueryValidator>();
        services.AddScoped<IValidator<CreateCommentCommand>, CreateCommentValidator>();
        services.AddScoped<IValidator<UpdateCommentCommand>, UpdateCommentValidator>();
        services.AddScoped<IValidator<GetAllCommentsQuery>, GetAllCommentsQueryValidator>();
        services.AddScoped<IValidator<GetCommentsByStockIdQuery>, GetCommentsByStockIdQueryValidator>();

        // Auth command validators
        services.AddScoped<IValidator<RegisterCommand>, RegisterCommandValidator>();
        services.AddScoped<IValidator<LoginCommand>, LoginCommandValidator>();
        services.AddScoped<IValidator<UpdateProfileCommand>, UpdateProfileCommandValidator>();
        services.AddScoped<IValidator<ChangePasswordCommand>, ChangePasswordCommandValidator>();
        services.AddScoped<IValidator<AssignRoleCommand>, AssignRoleCommandValidator>();

        // MediatorFlow pipeline behaviors
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}
