using ImportamosSports.Api.Data;
using ImportamosSports.Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ImportamosSports.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MarcasController : ControllerBase
{
    private readonly AppDbContext _context;

    public MarcasController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult> Listar()
    {
        var marcas = await _context.Marcas
            .OrderBy(m => m.Nombre)
            .ToListAsync();

        return Ok(marcas);
    }

    [Authorize(Roles = "Admin,Trabajador")]
    [HttpPost]
    public async Task<ActionResult> Crear([FromBody] Marca request)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre))
            return BadRequest("El nombre de la marca es obligatorio.");

        var nombre = request.Nombre.Trim();

        var existe = await _context.Marcas
            .AnyAsync(m => m.Nombre.ToLower() == nombre.ToLower());

        if (existe)
            return BadRequest("Ya existe una marca con ese nombre.");

        var marca = new Marca
        {
            Nombre = nombre
        };

        _context.Marcas.Add(marca);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            mensaje = "Marca registrada correctamente.",
            marca
        });
    }
}