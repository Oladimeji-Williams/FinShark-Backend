using MediatR;

namespace FinShark.Application.Auth.Commands.TestSmtp;

public sealed record TestSmtpCommand() : IRequest<string>;
