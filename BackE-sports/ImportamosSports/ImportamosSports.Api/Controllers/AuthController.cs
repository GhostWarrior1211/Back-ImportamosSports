using ImportamosSports.Api.Data;
using ImportamosSports.Api.DTOs;
using ImportamosSports.Api.Entities;
using ImportamosSports.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ImportamosSports.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly JwtService _jwtService;

    public AuthController(AppDbContext context, JwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        var usuario = await _context.Usuarios
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.Correo == request.Correo && u.Clave == request.Clave);

        if (usuario == null)
            return Unauthorized("Correo o clave incorrectos.");

        if (!usuario.Activo)
            return Unauthorized("La cuenta se encuentra desactivada.");

        var token = _jwtService.GenerarToken(usuario);

        return Ok(new LoginResponse
        {
            Token = token,
            Correo = usuario.Correo,
            Rol = usuario.Rol?.Nombre ?? "",
            NombreCompleto = $"{usuario.Nombres} {usuario.Apellidos}"
        });
    }

    [AllowAnonymous]
    [HttpPost("registrar-cliente")]
    public async Task<ActionResult> RegistrarCliente(RegistrarUsuarioRequest request)
    {
        var existe = await _context.Usuarios.AnyAsync(u => u.Correo == request.Correo);
        if (existe)
            return BadRequest("El correo ya está registrado.");

        var usuario = new Usuario
        {
            Nombres = request.Nombres,
            Apellidos = request.Apellidos,
            Correo = request.Correo,
            Clave = request.Clave,
            Telefono = request.Telefono,
            Activo = true,
            RolId = 3
        };

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        return Ok("Cliente registrado correctamente.");
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("registrar-trabajador")]
    public async Task<ActionResult> RegistrarTrabajador(RegistrarUsuarioRequest request)
    {
        var existe = await _context.Usuarios.AnyAsync(u => u.Correo == request.Correo);
        if (existe)
            return BadRequest("El correo ya está registrado.");

        var usuario = new Usuario
        {
            Nombres = request.Nombres,
            Apellidos = request.Apellidos,
            Correo = request.Correo,
            Clave = request.Clave,
            Telefono = request.Telefono,
            Activo = true,
            RolId = 2
        };

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        return Ok("Trabajador registrado correctamente.");
    }

    [Authorize]
    [HttpGet("perfil")]
    public IActionResult Perfil()
    {
        return Ok(new
        {
            Usuario = User.Identity?.Name,
            Correo = User.Claims.FirstOrDefault(c => c.Type.Contains("email"))?.Value,
            Rol = User.Claims.FirstOrDefault(c => c.Type.Contains("role"))?.Value
        });
    }
}