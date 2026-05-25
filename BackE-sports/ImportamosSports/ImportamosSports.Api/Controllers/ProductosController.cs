using ImportamosSports.Api.Data;
using ImportamosSports.Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ImportamosSports.Api.DTOs;

namespace ImportamosSports.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductosController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProductosController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult> GetProductos()
    {
        var productos = await _context.Productos
            .Include(p => p.Marca)
            .Where(p => p.Activo)
            .ToListAsync();

        return Ok(productos);
    }

    [Authorize(Roles = "Admin,Trabajador")]
    [HttpGet("admin")]
    public async Task<ActionResult> GetProductosAdmin()
    {
        var productos = await _context.Productos
            .Include(p => p.Marca)
            .Select(p => new
            {
                p.Id,
                p.Nombre,
                p.Descripcion,
                p.Precio,
                p.Stock,
                p.ImagenUrl,
                p.Activo,
                p.MarcaId,
                Marca = p.Marca,
                TienePedidos = _context.DetallesPedido.Any(d => d.ProductoId == p.Id)
            })
            .ToListAsync();

        return Ok(productos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Producto>> GetProducto(int id)
    {
        var producto = await _context.Productos
            .Include(p => p.Marca)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (producto == null)
            return NotFound("Producto no encontrado.");

        return Ok(producto);
    }

    [Authorize(Roles = "Admin,Trabajador")]
    [HttpPost]
    public async Task<ActionResult<Producto>> PostProducto(Producto producto)
    {
        var marcaExiste = await _context.Marcas.AnyAsync(m => m.Id == producto.MarcaId);
        if (!marcaExiste)
            return BadRequest("La marca no existe.");

        producto.Id = 0;
        producto.Marca = null;

        _context.Productos.Add(producto);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetProducto), new { id = producto.Id }, producto);
    }

    [Authorize(Roles = "Admin,Trabajador")]
    [HttpPut("{id}")]
    public async Task<ActionResult> PutProducto(int id, Producto producto)
    {
        if (id != producto.Id)
            return BadRequest("El id del producto no coincide.");

        var productoDb = await _context.Productos.FindAsync(id);
        if (productoDb == null)
            return NotFound("Producto no encontrado.");

        var marcaExiste = await _context.Marcas.AnyAsync(m => m.Id == producto.MarcaId);
        if (!marcaExiste)
            return BadRequest("La marca no existe.");

        productoDb.Nombre = producto.Nombre;
        productoDb.Descripcion = producto.Descripcion;
        productoDb.Precio = producto.Precio;
        productoDb.Stock = producto.Stock;
        productoDb.ImagenUrl = producto.ImagenUrl;
        productoDb.Activo = producto.Activo;
        productoDb.MarcaId = producto.MarcaId;

        await _context.SaveChangesAsync();

        return Ok(new { mensaje = "Producto actualizado correctamente." });
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteProducto(int id)
    {
        var producto = await _context.Productos.FindAsync(id);
        if (producto == null)
            return NotFound("Producto no encontrado.");

        var tienePedidos = await _context.DetallesPedido
            .AnyAsync(d => d.ProductoId == id);

        if (tienePedidos)
            return BadRequest("No se puede eliminar porque el producto ya tiene pedidos. Desactívalo.");

        _context.Productos.Remove(producto);
        await _context.SaveChangesAsync();

        return Ok(new { mensaje = "Producto eliminado correctamente." });
    }

    [Authorize(Roles = "Admin,Trabajador")]
    [HttpPut("{id}/activo")]
    public async Task<ActionResult> CambiarEstadoProducto(int id, CambiarEstadoProductoRequest request)
    {
        var producto = await _context.Productos.FindAsync(id);
        if (producto == null)
            return NotFound("Producto no encontrado.");

        producto.Activo = request.Activo;
        await _context.SaveChangesAsync();

        return Ok(new
        {
            mensaje = request.Activo
                ? "Producto activado correctamente."
                : "Producto desactivado correctamente."
        });
    }
}