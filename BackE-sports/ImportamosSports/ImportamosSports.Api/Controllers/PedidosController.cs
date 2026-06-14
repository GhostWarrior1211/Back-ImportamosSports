using ImportamosSports.Api.Data;
using ImportamosSports.Api.DTOs;
using ImportamosSports.Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ImportamosSports.Api.Services;

namespace ImportamosSports.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PedidosController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly WhatsAppService _whatsAppService;

    public PedidosController(AppDbContext context, WhatsAppService whatsAppService)
    {
        _context = context;
        _whatsAppService = whatsAppService;
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

            if (item.TallaUs <= 0)
                return BadRequest($"Debes seleccionar una talla para el producto {producto.Nombre}.");

            var tallaProducto = await _context.ProductoTallas
                .FirstOrDefaultAsync(t => t.ProductoId == producto.Id && t.TallaUs == item.TallaUs);

            if (tallaProducto == null)
                return BadRequest($"La talla US {item.TallaUs} no existe para el producto {producto.Nombre}.");

            if (tallaProducto.Stock < item.Cantidad)
                return BadRequest($"Stock insuficiente para {producto.Nombre} en talla US {item.TallaUs}.");

            if (producto.Stock < item.Cantidad)
                return BadRequest($"Stock insuficiente para el producto {producto.Nombre}.");

            tallaProducto.Stock -= item.Cantidad;
            producto.Stock -= item.Cantidad;

            var subTotalItem = producto.Precio * item.Cantidad;
            subtotal += subTotalItem;

            detallesPedido.Add(new DetallePedido
            {
                ProductoId = producto.Id,
                Cantidad = item.Cantidad,
                PrecioUnitario = producto.Precio,
                SubTotal = subTotalItem,
                TallaUs = item.TallaUs
            });
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

        var metodoPago = string.IsNullOrWhiteSpace(request.MetodoPago)
            ? "Tarjeta"
            : request.MetodoPago.Trim();

        var estadoPago = metodoPago.Equals("Tarjeta", StringComparison.OrdinalIgnoreCase)
            ? "Confirmado"
            : "Pendiente";

        var estadoPedidoInicialId = metodoPago.Equals("Tarjeta", StringComparison.OrdinalIgnoreCase)
            ? 2
            : 1;
        var tipoComprobante = string.IsNullOrWhiteSpace(request.TipoComprobante)
            ? "Boleta"
            : request.TipoComprobante.Trim();

        if (tipoComprobante == "Factura")
        {
            if (string.IsNullOrWhiteSpace(request.RucFactura) || request.RucFactura.Length != 11)
                return BadRequest("El RUC debe tener exactamente 11 números.");

            if (string.IsNullOrWhiteSpace(request.RazonSocialFactura))
                return BadRequest("La razón social es obligatoria.");

            if (string.IsNullOrWhiteSpace(request.DireccionFiscalFactura))
                return BadRequest("La dirección fiscal es obligatoria.");
        }
        var pedido = new Pedido
        {
            UsuarioId = usuarioId,
            EstadoPedidoId = estadoPedidoInicialId,
            CuponId = cuponId,
            Subtotal = subtotal,
            DescuentoAplicado = descuentoAplicado,
            Igv = igv,
            Total = total,
            Fecha = DateTime.Now,

            MetodoPago = metodoPago,
            EstadoPago = estadoPago,
            AsesorVentaId = request.AsesorVentaId,
            ObservacionPago = request.ObservacionPago,
            NumeroOperacion = request.NumeroOperacion,

            TipoComprobante = tipoComprobante,
            RucFactura = tipoComprobante == "Factura" ? request.RucFactura : null,
            RazonSocialFactura = tipoComprobante == "Factura" ? request.RazonSocialFactura : null,
            DireccionFiscalFactura = tipoComprobante == "Factura" ? request.DireccionFiscalFactura : null,

            Detalles = detallesPedido
        };

        _context.Pedidos.Add(pedido);
        await _context.SaveChangesAsync();

        pedido.SerieComprobante = tipoComprobante == "Factura" ? "F001" : "B001";
        pedido.NumeroComprobante = pedido.Id;

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
            total = pedido.Total,
            tipoComprobante = pedido.TipoComprobante,
            serieComprobante = pedido.SerieComprobante,
            numeroComprobante = pedido.NumeroComprobante
        });
    }

    [Authorize(Roles = "Admin,Trabajador")]
    [HttpGet]
    public async Task<ActionResult> ListarPedidos()
    {
        var pedidos = await _context.Pedidos
            .Include(p => p.Usuario)
            .Include(p => p.EstadoPedido)
            .Include(p => p.AsesorVenta)
            .Include(p => p.Detalles)
                .ThenInclude(d => d.Producto)
            .OrderByDescending(p => p.Id)
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
            .Include(p => p.AsesorVenta)
            .Include(p => p.Detalles)
                .ThenInclude(d => d.Producto)
            .Where(p => p.UsuarioId == usuarioId)
            .OrderByDescending(p => p.Id)
            .ToListAsync();

        return Ok(pedidos);
    }

    [Authorize(Roles = "Admin,Trabajador")]
    [HttpPut("{id}/estado/{estadoId}")]
    public async Task<ActionResult> CambiarEstado(int id, int estadoId)
    {
        var pedido = await _context.Pedidos
            .Include(p => p.Usuario)
            .Include(p => p.EstadoPedido)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (pedido == null)
            return NotFound("Pedido no encontrado.");

        var estado = await _context.EstadosPedido.FindAsync(estadoId);
        if (estado == null)
            return NotFound("Estado no encontrado.");

        if (estado.Nombre == "Cancelado")
            return BadRequest("El pedido solo se puede cancelar usando el botón Cancelar compra.");

        if (pedido.EstadoPedido?.Nombre == "Cancelado")
            return BadRequest("No se puede cambiar el estado de un pedido cancelado.");

        if (pedido.EstadoPedido?.Nombre == "Entregado")
            return BadRequest("No se puede cambiar el estado de un pedido entregado.");

        if (estadoId < pedido.EstadoPedidoId)
            return BadRequest("No puedes volver a un estado anterior.");

        if (estadoId == pedido.EstadoPedidoId)
            return BadRequest("El pedido ya se encuentra en ese estado.");

        pedido.EstadoPedidoId = estadoId;

        if (estado.Nombre == "Pagado")
        {
            pedido.EstadoPago = "Confirmado";
        }

        string mensaje = estado.Nombre switch
        {
            "Pendiente" => $"Tu pedido #{pedido.Id} ha sido registrado y está pendiente de atención.",
            "Pagado" => $"Tu pedido #{pedido.Id} fue pagado correctamente.",
            "En preparación" => $"Tu pedido #{pedido.Id} se encuentra en preparación.",
            "En camino" => $"Tu pedido #{pedido.Id} está en camino.",
            "Entregado" => $"Tu pedido #{pedido.Id} fue entregado correctamente.",
            _ => $"Tu pedido #{pedido.Id} cambió al estado: {estado.Nombre}."
        };

        var notificacion = new Notificacion
        {
            PedidoId = pedido.Id,
            Medio = "WhatsApp Cloud API",
            Mensaje = mensaje,
            Fecha = DateTime.Now,
            Enviado = false
        };

        _context.Notificaciones.Add(notificacion);
        await _context.SaveChangesAsync();

        var whatsappEnviado = false;
        var telefonoCliente = pedido.Usuario?.Telefono;

        if (!string.IsNullOrWhiteSpace(telefonoCliente))
        {
            whatsappEnviado = await _whatsAppService.EnviarCambioEstadoPedido(
                telefonoCliente,
                pedido.Usuario?.Nombres ?? "cliente",
                pedido.Id,
                estado.Nombre
            );
        }

        notificacion.Enviado = whatsappEnviado;
        await _context.SaveChangesAsync();

        return Ok(new
        {
            mensaje = "Estado del pedido actualizado correctamente.",
            notificacion = notificacion.Mensaje,
            whatsappEnviado
        });
    }

    [Authorize(Roles = "Admin,Trabajador")]
    [HttpPut("{id}/confirmar-compra")]
    public async Task<IActionResult> ConfirmarCompra(int id, [FromBody] ConfirmarCompraRequest request)
    {
        var pedido = await _context.Pedidos
            .Include(p => p.Usuario)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (pedido == null)
            return NotFound("Pedido no encontrado.");

        var estadoPagado = await _context.EstadosPedido
            .FirstOrDefaultAsync(x => x.Nombre == "Pagado");

        if (estadoPagado == null)
            return BadRequest("No existe el estado Pagado.");

        pedido.EstadoPago = "Confirmado";
        pedido.NumeroOperacion = request.NumeroOperacion;
        pedido.ObservacionPago = request.ObservacionPago;
        pedido.EstadoPedidoId = estadoPagado.Id;

        var notificacion = new Notificacion
        {
            PedidoId = pedido.Id,
            Medio = "WhatsApp Cloud API",
            Mensaje = $"Tu pedido #{pedido.Id} fue confirmado correctamente.",
            Fecha = DateTime.Now,
            Enviado = false
        };

        _context.Notificaciones.Add(notificacion);
        await _context.SaveChangesAsync();

        var whatsappEnviado = false;
        var telefonoCliente = pedido.Usuario?.Telefono;

        if (!string.IsNullOrWhiteSpace(telefonoCliente))
        {
            whatsappEnviado = await _whatsAppService.EnviarCambioEstadoPedido(
                telefonoCliente,
                pedido.Usuario?.Nombres ?? "cliente",
                pedido.Id,
                "Pagado"
            );
        }

        notificacion.Enviado = whatsappEnviado;
        await _context.SaveChangesAsync();

        return Ok(new
        {
            mensaje = "Compra confirmada correctamente.",
            notificacion = notificacion.Mensaje,
            whatsappEnviado
        });
    }

    [Authorize(Roles = "Admin,Trabajador")]
    [HttpPut("{id}/cancelar-compra")]
    public async Task<IActionResult> CancelarCompra(int id, [FromBody] CancelarCompraRequest request)
    {
        var pedido = await _context.Pedidos
            .Include(p => p.Usuario)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (pedido == null)
            return NotFound("Pedido no encontrado.");

        var estadoCancelado = await _context.EstadosPedido
            .FirstOrDefaultAsync(x => x.Nombre == "Cancelado");

        if (estadoCancelado == null)
            return BadRequest("No existe el estado Cancelado.");

        pedido.EstadoPago = "Cancelado";
        pedido.ObservacionPago = request.ObservacionPago;
        pedido.EstadoPedidoId = estadoCancelado.Id;

        var motivo = string.IsNullOrWhiteSpace(request.ObservacionPago)
            ? "Pedido cancelado"
            : request.ObservacionPago;

        var notificacion = new Notificacion
        {
            PedidoId = pedido.Id,
            Medio = "WhatsApp Cloud API",
            Mensaje = $"Tu pedido #{pedido.Id} fue cancelado. Motivo: {motivo}",
            Fecha = DateTime.Now,
            Enviado = false
        };

        _context.Notificaciones.Add(notificacion);
        await _context.SaveChangesAsync();

        var whatsappEnviado = false;
        var telefonoCliente = pedido.Usuario?.Telefono;

        if (!string.IsNullOrWhiteSpace(telefonoCliente))
        {
            whatsappEnviado = await _whatsAppService.EnviarCambioEstadoPedido(
                telefonoCliente,
                pedido.Usuario?.Nombres ?? "cliente",
                pedido.Id,
                "Cancelado",
                motivo
            );
        }

        notificacion.Enviado = whatsappEnviado;
        await _context.SaveChangesAsync();

        return Ok(new
        {
            mensaje = "Compra cancelada correctamente.",
            notificacion = notificacion.Mensaje,
            whatsappEnviado
        });
    }
    [Authorize]
    [HttpGet("{id}/comprobante")]
    public async Task<ActionResult> ObtenerComprobante(int id)
    {
        var pedido = await _context.Pedidos
            .Include(p => p.Usuario)
            .Include(p => p.EstadoPedido)
            .Include(p => p.Detalles)
                .ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (pedido == null)
            return NotFound("Pedido no encontrado.");

        var rol = User.FindFirst(ClaimTypes.Role)?.Value;
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (rol == "Cliente")
        {
            if (string.IsNullOrWhiteSpace(usuarioIdClaim))
                return Unauthorized("Token inválido.");

            var usuarioId = int.Parse(usuarioIdClaim);

            if (pedido.UsuarioId != usuarioId)
                return Forbid();
        }

        if (pedido.EstadoPago != "Confirmado")
            return BadRequest("Solo se puede imprimir comprobante de pagos confirmados.");

        return Ok(pedido);
    }
}