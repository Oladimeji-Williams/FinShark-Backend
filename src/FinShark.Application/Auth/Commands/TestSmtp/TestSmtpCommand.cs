using MediatorFlow.Core.Contracts;

namespace FinShark.Application.Auth.Commands.TestSmtp;

public sealed record TestSmtpCommand() : IRequest<string>;
