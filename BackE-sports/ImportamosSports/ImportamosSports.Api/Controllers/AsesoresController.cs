using ImportamosSports.Api.Data;
using ImportamosSports.Api.DTOs;
using ImportamosSports.Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ImportamosSports.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AsesoresController : ControllerBase
{
    private readonly AppDbContext _context;

    public AsesoresController(AppDbContext context)
    {
        _context = context;
    }

    // Cliente: ver asesores activos
    [HttpGet("disponibles")]
    public async Task<ActionResult> GetDisponibles()
    {
        var asesores = await _context.AsesoresVenta
            .Where(a => a.Activo)
            .OrderBy(a => a.Nombres)
            .ToListAsync();

        return Ok(asesores);
    }

    // Admin/Trabajador: ver todos
    [Authorize(Roles = "Admin,Trabajador")]
    [HttpGet]
    public async Task<ActionResult> GetTodos()
    {
        var asesores = await _context.AsesoresVenta
            .OrderBy(a => a.Nombres)
            .ToListAsync();

        return Ok(asesores);
    }

    // Admin/Trabajador: crear
    [Authorize(Roles = "Admin,Trabajador")]
    [HttpPost]
    public async Task<ActionResult> Crear(CrearAsesorVentaRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nombres))
            return BadRequest("Los nombres son obligatorios.");

        if (string.IsNullOrWhiteSpace(request.Apellidos))
            return BadRequest("Los apellidos son obligatorios.");

        if (string.IsNullOrWhiteSpace(request.Telefono))
            return BadRequest("El teléfono es obligatorio.");

        var asesor = new AsesorVenta
        {
            Nombres = request.Nombres.Trim(),
            Apellidos = request.Apellidos.Trim(),
            Telefono = request.Telefono.Trim(),
            FotoUrl = request.FotoUrl,
            Activo = request.Activo
        };

        _context.AsesoresVenta.Add(asesor);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            mensaje = "Asesor creado correctamente.",
            asesorId = asesor.Id
        });
    }

    // Admin/Trabajador: editar
    [Authorize(Roles = "Admin,Trabajador")]
    [HttpPut("{id}")]
    public async Task<ActionResult> Editar(int id, CrearAsesorVentaRequest request)
    {
        var asesor = await _context.AsesoresVenta.FindAsync(id);
        if (asesor == null)
            return NotFound("Asesor no encontrado.");

        asesor.Nombres = request.Nombres.Trim();
        asesor.Apellidos = request.Apellidos.Trim();
        asesor.Telefono = request.Telefono.Trim();
        asesor.FotoUrl = request.FotoUrl;
        asesor.Activo = request.Activo;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            mensaje = "Asesor actualizado correctamente."
        });
    }

    // Admin/Trabajador: activar/desactivar
    [Authorize(Roles = "Admin,Trabajador")]
    [HttpPut("{id}/estado")]
    public async Task<ActionResult> CambiarEstado(int id, CambiarEstadoAsesorRequest request)
    {
        var asesor = await _context.AsesoresVenta.FindAsync(id);
        if (asesor == null)
            return NotFound("Asesor no encontrado.");

        asesor.Activo = request.Activo;
        await _context.SaveChangesAsync();

        return Ok(new
        {
            mensaje = request.Activo
                ? "Asesor activado correctamente."
                : "Asesor desactivado correctamente."
        });
    }

    // Admin: eliminar
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<ActionResult> Eliminar(int id)
    {
        var asesor = await _context.AsesoresVenta.FindAsync(id);
        if (asesor == null)
            return NotFound("Asesor no encontrado.");

        _context.AsesoresVenta.Remove(asesor);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            mensaje = "Asesor eliminado correctamente."
        });
    }
}