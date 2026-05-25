using ImportamosSports.Api.Data;
using ImportamosSports.Api.DTOs;
using ImportamosSports.Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ImportamosSports.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CuponesController : ControllerBase
{
    private readonly AppDbContext _context;

    public CuponesController(AppDbContext context)
    {
        _context = context;
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult> CrearCupon(CrearCuponRequest request)
    {
        var codigo = request.Codigo.Trim().ToUpper();

        if (string.IsNullOrWhiteSpace(codigo))
            return BadRequest("El código es obligatorio.");

        if (request.Valor <= 0)
            return BadRequest("El valor del cupón debe ser mayor a 0.");

        if (request.FechaFin < request.FechaInicio)
            return BadRequest("La fecha fin no puede ser menor que la fecha inicio.");

        if (request.TipoDescuento != "Porcentaje" && request.TipoDescuento != "MontoFijo")
            return BadRequest("El tipo de descuento debe ser 'Porcentaje' o 'MontoFijo'.");

        var existe = await _context.Cupones.AnyAsync(x => x.Codigo == codigo);
        if (existe)
            return BadRequest("Ya existe un cupón con ese código.");

        var cupon = new Cupon
        {
            Codigo = codigo,
            TipoDescuento = request.TipoDescuento,
            Valor = request.Valor,
            Activo = request.Activo,
            FechaInicio = request.FechaInicio,
            FechaFin = request.FechaFin,
            CompraMinima = request.CompraMinima
        };

        _context.Cupones.Add(cupon);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            mensaje = "Cupón creado correctamente.",
            cuponId = cupon.Id
        });
    }

    [Authorize(Roles = "Admin,Trabajador")]
    [HttpGet]
    public async Task<ActionResult> ListarCupones()
    {
        var cupones = await _context.Cupones
            .OrderByDescending(x => x.Id)
            .ToListAsync();

        return Ok(cupones);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}/estado")]
    public async Task<ActionResult> CambiarEstado(int id, CambiarEstadoCuponRequest request)
    {
        var cupon = await _context.Cupones.FindAsync(id);
        if (cupon == null)
            return NotFound("Cupón no encontrado.");

        cupon.Activo = request.Activo;
        await _context.SaveChangesAsync();

        return Ok(new
        {
            mensaje = request.Activo
                ? "Cupón activado correctamente."
                : "Cupón desactivado correctamente."
        });
    }

    [Authorize(Roles = "Cliente")]
    [HttpPost("validar")]
    public async Task<ActionResult> ValidarCupon(ValidarCuponRequest request)
    {
        var codigo = request.Codigo.Trim().ToUpper();

        if (string.IsNullOrWhiteSpace(codigo))
            return BadRequest("Debes ingresar un código.");

        var hoy = DateTime.Now;

        var cupon = await _context.Cupones
            .FirstOrDefaultAsync(x => x.Codigo == codigo);

        if (cupon == null)
        {
            return Ok(new
            {
                esValido = false,
                mensaje = "Cupón no encontrado.",
                descuentoAplicado = 0m
            });
        }

        if (!cupon.Activo)
        {
            return Ok(new
            {
                esValido = false,
                mensaje = "El cupón está inactivo.",
                descuentoAplicado = 0m
            });
        }

        if (hoy < cupon.FechaInicio || hoy > cupon.FechaFin)
        {
            return Ok(new
            {
                esValido = false,
                mensaje = "El cupón está fuera de vigencia.",
                descuentoAplicado = 0m
            });
        }

        if (request.Subtotal < cupon.CompraMinima)
        {
            return Ok(new
            {
                esValido = false,
                mensaje = $"La compra mínima para este cupón es S/ {cupon.CompraMinima:0.00}.",
                descuentoAplicado = 0m
            });
        }

        decimal descuentoAplicado = 0m;

        if (cupon.TipoDescuento == "Porcentaje")
        {
            descuentoAplicado = request.Subtotal * (cupon.Valor / 100m);
        }
        else if (cupon.TipoDescuento == "MontoFijo")
        {
            descuentoAplicado = cupon.Valor;
        }

        if (descuentoAplicado > request.Subtotal)
            descuentoAplicado = request.Subtotal;

        descuentoAplicado = Math.Round(descuentoAplicado, 2);

        return Ok(new
        {
            esValido = true,
            mensaje = "Cupón válido.",
            descuentoAplicado,
            tipoDescuento = cupon.TipoDescuento,
            valor = cupon.Valor,
            codigo = cupon.Codigo
        });
    }
    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<ActionResult> ActualizarCupon(int id, CrearCuponRequest request)
    {
        var cupon = await _context.Cupones.FindAsync(id);
        if (cupon == null)
            return NotFound("Cupón no encontrado.");

        var existeCodigo = await _context.Cupones
            .AnyAsync(c => c.Id != id && c.Codigo == request.Codigo);

        if (existeCodigo)
            return BadRequest("Ya existe otro cupón con ese código.");

        cupon.Codigo = request.Codigo.Trim().ToUpper();
        cupon.TipoDescuento = request.TipoDescuento;
        cupon.Valor = request.Valor;
        cupon.Activo = request.Activo;
        cupon.FechaInicio = request.FechaInicio;
        cupon.FechaFin = request.FechaFin;
        cupon.CompraMinima = request.CompraMinima;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            mensaje = "Cupón actualizado correctamente."
        });
    }
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<ActionResult> EliminarCupon(int id)
    {
        var cupon = await _context.Cupones.FindAsync(id);
        if (cupon == null)
            return NotFound("Cupón no encontrado.");

        _context.Cupones.Remove(cupon);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            mensaje = "Cupón eliminado correctamente."
        });
    }
}