using MediatR;

namespace FodmapLog.Server.Commands
{
    public record LoginCommand(string Email, string Password) : IRequest<string>;
}