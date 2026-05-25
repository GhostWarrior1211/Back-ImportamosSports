using ImportamosSports.Api.Data;
using ImportamosSports.Api.DTOs;
using ImportamosSports.Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ImportamosSports.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PedidosController : ControllerBase
{
    private readonly AppDbContext _context;

    public PedidosController(AppDbContext context)
    {
        _context = context;
    }

    [Authorize(Roles = "Cliente")]
    [HttpPost]
    public async Task<ActionResult> CrearPedido(CrearPedidoRequest request)
    {
        if (request.Detalles == null || !request.Detalles.Any())
            return BadRequest("El pedido no tiene productos.");

        var usuarioIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(usuarioIdClaim))
            return Unauthorized("Token inválido.");

        var usuarioId = int.Parse(usuarioIdClaim);

        var usuario = await _context.Usuarios.FindAsync(usuarioId);
        if (usuario == null)
            return NotFound("Usuario no encontrado.");

        decimal subtotal = 0;
        decimal descuentoAplicado = 0;
        decimal igv = 0;
        decimal total = 0;
        int? cuponId = null;

        var detallesPedido = new List<DetallePedido>();

        foreach (var item in request.Detalles)
        {
            var producto = await _context.Productos.FindAsync(item.ProductoId);
            if (producto == null)
                return NotFound($"Producto con id {item.ProductoId} no encontrado.");

            if (producto.Stock < item.Cantidad)
                return BadRequest($"Stock insuficiente para el producto {producto.Nombre}.");

            var subTotalItem = producto.Precio * item.Cantidad;
            subtotal += subTotalItem;

            detallesPedido.Add(new DetallePedido
            {
                ProductoId = producto.Id,
                Cantidad = item.Cantidad,
                PrecioUnitario = producto.Precio,
                SubTotal = subTotalItem
            });

            producto.Stock -= item.Cantidad;
        }

        subtotal = Math.Round(subtotal, 2);

        if (!string.IsNullOrWhiteSpace(request.CodigoCupon))
        {
            var codigo = request.CodigoCupon.Trim().ToUpper();
            var hoy = DateTime.Now;

            var cupon = await _context.Cupones.FirstOrDefaultAsync(x => x.Codigo == codigo);

            if (cupon == null)
                return BadRequest("Cupón no encontrado.");

            if (!cupon.Activo)
                return BadRequest("El cupón está inactivo.");

            if (hoy < cupon.FechaInicio || hoy > cupon.FechaFin)
                return BadRequest("El cupón está fuera de vigencia.");

            if (subtotal < cupon.CompraMinima)
                return BadRequest($"La compra mínima para este cupón es S/ {cupon.CompraMinima:0.00}.");

            if (cupon.TipoDescuento == "Porcentaje")
            {
                descuentoAplicado = subtotal * (cupon.Valor / 100m);
            }
            else if (cupon.TipoDescuento == "MontoFijo")
            {
                descuentoAplicado = cupon.Valor;
            }

            if (descuentoAplicado > subtotal)
                descuentoAplicado = subtotal;

            descuentoAplicado = Math.Round(descuentoAplicado, 2);
            cuponId = cupon.Id;
        }

        var baseImponible = subtotal - descuentoAplicado;
        if (baseImponible < 0)
            baseImponible = 0;

        igv = Math.Round(baseImponible * 0.18m, 2);
        total = Math.Round(baseImponible + igv, 2);

        var pedido = new Pedido
        {
            UsuarioId = usuarioId,
            EstadoPedidoId = 1,
            CuponId = cuponId,
            Subtotal = subtotal,
            DescuentoAplicado = descuentoAplicado,
            Igv = igv,
            Total = total,
            Fecha = DateTime.Now,
            Detalles = detallesPedido
        };

        _context.Pedidos.Add(pedido);
        await _context.SaveChangesAsync();

        var notificacionInicial = new Notificacion
        {
            PedidoId = pedido.Id,
            Medio = "WhatsApp Simulado",
            Mensaje = $"Tu pedido #{pedido.Id} ha sido registrado correctamente.",
            Fecha = DateTime.Now,
            Enviado = true
        };

        _context.Notificaciones.Add(notificacionInicial);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            mensaje = "Pedido registrado correctamente.",
            pedidoId = pedido.Id,
            subtotal = pedido.Subtotal,
            descuentoAplicado = pedido.DescuentoAplicado,
            igv = pedido.Igv,
            total = pedido.Total
        });
    }

    [Authorize(Roles = "Admin,Trabajador")]
    [HttpGet]
    public async Task<ActionResult> ListarPedidos()
    {
        var pedidos = await _context.Pedidos
            .Include(p => p.Usuario)
            .Include(p => p.EstadoPedido)
            .Include(p => p.Detalles)
                .ThenInclude(d => d.Producto)
            .ToListAsync();

        return Ok(pedidos);
    }

    [Authorize(Roles = "Cliente")]
    [HttpGet("mis-pedidos")]
    public async Task<ActionResult> MisPedidos()
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(usuarioIdClaim))
            return Unauthorized("Token inválido.");

        var usuarioId = int.Parse(usuarioIdClaim);

        var pedidos = await _context.Pedidos
            .Include(p => p.EstadoPedido)
            .Include(p => p.Detalles)
                .ThenInclude(d => d.Producto)
            .Where(p => p.UsuarioId == usuarioId)
            .ToListAsync();

        return Ok(pedidos);
    }

    [Authorize(Roles = "Admin,Trabajador")]
    [HttpPut("{id}/estado/{estadoId}")]
    public async Task<ActionResult> CambiarEstado(int id, int estadoId)
    {
        var pedido = await _context.Pedidos
            .Include(p => p.Usuario)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (pedido == null)
            return NotFound("Pedido no encontrado.");

        var estado = await _context.EstadosPedido.FindAsync(estadoId);
        if (estado == null)
            return NotFound("Estado no encontrado.");

        pedido.EstadoPedidoId = estadoId;
        await _context.SaveChangesAsync();

        string mensaje = estado.Nombre switch
        {
            "Pendiente" => $"Tu pedido #{pedido.Id} ha sido registrado y está pendiente de atención.",
            "Pagado" => $"Tu pedido #{pedido.Id} fue pagado correctamente.",
            "En preparación" => $"Tu pedido #{pedido.Id} se encuentra en preparación.",
            "En camino" => $"Tu pedido #{pedido.Id} está en camino.",
            "Entregado" => $"Tu pedido #{pedido.Id} fue entregado. Gracias por tu compra.",
            _ => $"Tu pedido #{pedido.Id} cambió al estado: {estado.Nombre}."
        };

        var notificacion = new Notificacion
        {
            PedidoId = pedido.Id,
            Medio = "WhatsApp Simulado",
            Mensaje = mensaje,
            Fecha = DateTime.Now,
            Enviado = true
        };

        _context.Notificaciones.Add(notificacion);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            mensaje = "Estado del pedido actualizado correctamente.",
            notificacion = notificacion.Mensaje
        });
    }
}