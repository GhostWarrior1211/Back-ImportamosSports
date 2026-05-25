using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ImportamosSports.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UploadsController : ControllerBase
{
    private readonly IWebHostEnvironment _env;

    public UploadsController(IWebHostEnvironment env)
    {
        _env = env;
    }

    [Authorize(Roles = "Admin,Trabajador")]
    [HttpPost("producto")]
    public async Task<ActionResult> SubirImagenProducto(IFormFile archivo)
    {
        if (archivo == null || archivo.Length == 0)
            return BadRequest("No se envió ningún archivo.");

        var extensionesPermitidas = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();

        if (!extensionesPermitidas.Contains(extension))
            return BadRequest("Solo se permiten imágenes JPG, PNG o WEBP.");

        var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
        var carpetaDestino = Path.Combine(webRoot, "img", "productos");

        if (!Directory.Exists(carpetaDestino))
            Directory.CreateDirectory(carpetaDestino);

        var nombreArchivo = $"{Guid.NewGuid()}{extension}";
        var rutaCompleta = Path.Combine(carpetaDestino, nombreArchivo);

        using (var stream = new FileStream(rutaCompleta, FileMode.Create))
        {
            await archivo.CopyToAsync(stream);
        }

        var rutaRelativa = $"/img/productos/{nombreArchivo}";

        return Ok(new
        {
            mensaje = "Imagen subida correctamente.",
            imagenUrl = rutaRelativa
        });
    }
}