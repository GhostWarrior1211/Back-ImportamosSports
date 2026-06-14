using ImportamosSports.Api.Data;
using ImportamosSports.Api.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ImportamosSports.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class UsuariosController : ControllerBase
{
    private readonly AppDbContext _context;

    public UsuariosController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult> ListarUsuarios()
    {
        var usuarios = await _context.Usuarios
            .Include(u => u.Rol)
            .Where(u => u.RolId == 2 || u.RolId == 3)
            .OrderBy(u => u.RolId)
            .ThenBy(u => u.Nombres)
            .Select(u => new
            {
                u.Id,
                u.Nombres,
                u.Apellidos,
                u.Correo,
                u.Telefono,
                u.Activo,
                u.RolId,
                Rol = u.Rol == null ? null : new
                {
                    u.Rol.Id,
                    u.Rol.Nombre
                }
            })
            .ToListAsync();

        return Ok(usuarios);
    }

    [HttpGet("clientes")]
    public async Task<ActionResult> ListarClientes()
    {
        var usuarios = await _context.Usuarios
            .Include(u => u.Rol)
            .Where(u => u.RolId == 3)
            .OrderBy(u => u.Nombres)
            .Select(u => new
            {
                u.Id,
                u.Nombres,
                u.Apellidos,
                u.Correo,
                u.Telefono,
                u.Activo,
                u.RolId,
                Rol = u.Rol == null ? null : new
                {
                    u.Rol.Id,
                    u.Rol.Nombre
                }
            })
            .ToListAsync();

        return Ok(usuarios);
    }

    [HttpGet("trabajadores")]
    public async Task<ActionResult> ListarTrabajadores()
    {
        var usuarios = await _context.Usuarios
            .Include(u => u.Rol)
            .Where(u => u.RolId == 2)
            .OrderBy(u => u.Nombres)
            .Select(u => new
            {
                u.Id,
                u.Nombres,
                u.Apellidos,
                u.Correo,
                u.Telefono,
                u.Activo,
                u.RolId,
                Rol = u.Rol == null ? null : new
                {
                    u.Rol.Id,
                    u.Rol.Nombre
                }
            })
            .ToListAsync();

        return Ok(usuarios);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> ActualizarUsuario(int id, ActualizarUsuarioRequest request)
    {
        var usuario = await _context.Usuarios.FindAsync(id);

        if (usuario == null)
            return NotFound("Usuario no encontrado.");

        if (usuario.RolId == 1)
            return BadRequest("No se puede editar al usuario administrador desde esta pantalla.");

        if (request.RolId != 2 && request.RolId != 3)
            return BadRequest("Solo puedes asignar rol Trabajador o Cliente.");

        var correo = request.Correo.Trim().ToLower();

        var existeCorreo = await _context.Usuarios
            .AnyAsync(u => u.Id != id && u.Correo.ToLower() == correo);

        if (existeCorreo)
            return BadRequest("Ya existe otro usuario con ese correo.");

        usuario.Nombres = request.Nombres.Trim();
        usuario.Apellidos = request.Apellidos.Trim();
        usuario.Correo = correo;
        usuario.Telefono = request.Telefono.Trim();
        usuario.RolId = request.RolId;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            mensaje = "Usuario actualizado correctamente."
        });
    }

    [HttpPut("{id}/estado")]
    public async Task<ActionResult> CambiarEstadoUsuario(int id, CambiarEstadoUsuarioRequest request)
    {
        var usuario = await _context.Usuarios.FindAsync(id);

        if (usuario == null)
            return NotFound("Usuario no encontrado.");

        if (usuario.RolId == 1)
            return BadRequest("No se puede desactivar al administrador principal.");

        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!string.IsNullOrWhiteSpace(usuarioIdClaim)
            && int.Parse(usuarioIdClaim) == id
            && !request.Activo)
        {
            return BadRequest("No puedes desactivar tu propia cuenta.");
        }

        usuario.Activo = request.Activo;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            mensaje = request.Activo
                ? "Usuario activado correctamente."
                : "Usuario desactivado correctamente."
        });
    }
}