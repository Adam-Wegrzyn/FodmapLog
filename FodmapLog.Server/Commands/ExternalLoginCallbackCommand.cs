using MediatR;

namespace FodmapLog.Server.Commands
{
    public record ExternalLoginCallbackCommand(string ReturnUrl, string RemoteError): IRequest<string?>;
}