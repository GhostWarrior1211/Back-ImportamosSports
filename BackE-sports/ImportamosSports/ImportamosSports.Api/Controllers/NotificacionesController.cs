using ImportamosSports.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ImportamosSports.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificacionesController : ControllerBase
{
    private readonly AppDbContext _context;

    public NotificacionesController(AppDbContext context)
    {
        _context = context;
    }

    [Authorize(Roles = "Admin,Trabajador")]
    [HttpGet]
    public async Task<ActionResult> ListarNotificaciones()
    {
        var notificaciones = await _context.Notificaciones
            .OrderByDescending(n => n.Fecha)
            .ToListAsync();

        return Ok(notificaciones);
    }

    [Authorize(Roles = "Cliente")]
    [HttpGet("mis-notificaciones")]
    public async Task<ActionResult> MisNotificaciones()
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(usuarioIdClaim))
            return Unauthorized("Token inválido.");

        var usuarioId = int.Parse(usuarioIdClaim);

        var notificaciones = await _context.Notificaciones
            .Include(n => n.Pedido)
            .Where(n => n.Pedido != null && n.Pedido.UsuarioId == usuarioId)
            .OrderByDescending(n => n.Fecha)
            .ToListAsync();

        return Ok(notificaciones);
    }

    [Authorize(Roles = "Admin,Trabajador")]
    [HttpGet("pedido/{pedidoId}")]
    public async Task<ActionResult> ListarPorPedido(int pedidoId)
    {
        var notificaciones = await _context.Notificaciones
            .Where(n => n.PedidoId == pedidoId)
            .OrderBy(n => n.Fecha)
            .ToListAsync();

        return Ok(notificaciones);
    }
}