using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Arooaf.Api.Hubs;

[Authorize]
public class AlertasHub : Hub
{
}

public record AlertaCriticaMsg(
    string Tramo,
    double? Kilometraje,
    string Hora,
    string Severidad,
    string? FotoBase64
);
