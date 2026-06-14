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
            .Include(p => p.Tallas)
            .Where(p => p.Activo)
            .Select(p => new
            {
                id = p.Id,
                nombre = p.Nombre,
                descripcion = p.Descripcion,
                precio = p.Precio,
                stock = p.Stock,
                imagenUrl = p.ImagenUrl,
                activo = p.Activo,
                marcaId = p.MarcaId,
                marca = p.Marca == null ? null : new
                {
                    id = p.Marca.Id,
                    nombre = p.Marca.Nombre
                },
                tallas = p.Tallas
                    .OrderBy(t => t.TallaUs)
                    .Select(t => new
                    {
                        id = t.Id,
                        productoId = t.ProductoId,
                        tallaUs = t.TallaUs,
                        stock = t.Stock
                    })
                    .ToList()
            })
            .ToListAsync();

        return Ok(productos);
    }

    [Authorize(Roles = "Admin,Trabajador")]
    [HttpGet("admin")]
    public async Task<ActionResult> GetProductosAdmin()
    {
        var productos = await _context.Productos
            .Include(p => p.Marca)
            .Include(p => p.Tallas)
            .Select(p => new
            {
                id = p.Id,
                nombre = p.Nombre,
                descripcion = p.Descripcion,
                precio = p.Precio,
                stock = p.Stock,
                imagenUrl = p.ImagenUrl,
                activo = p.Activo,
                marcaId = p.MarcaId,
                marca = p.Marca == null ? null : new
                {
                    id = p.Marca.Id,
                    nombre = p.Marca.Nombre
                },
                tallas = p.Tallas
                    .OrderBy(t => t.TallaUs)
                    .Select(t => new
                    {
                        id = t.Id,
                        productoId = t.ProductoId,
                        tallaUs = t.TallaUs,
                        stock = t.Stock
                    })
                    .ToList(),
                tienePedidos = _context.DetallesPedido.Any(d => d.ProductoId == p.Id)
            })
            .ToListAsync();

        return Ok(productos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetProducto(int id)
    {
        var producto = await _context.Productos
            .Include(p => p.Marca)
            .Include(p => p.Tallas)
            .Where(p => p.Id == id)
            .Select(p => new
            {
                id = p.Id,
                nombre = p.Nombre,
                descripcion = p.Descripcion,
                precio = p.Precio,
                stock = p.Stock,
                imagenUrl = p.ImagenUrl,
                activo = p.Activo,
                marcaId = p.MarcaId,
                marca = p.Marca == null ? null : new
                {
                    id = p.Marca.Id,
                    nombre = p.Marca.Nombre
                },
                tallas = p.Tallas
                    .OrderBy(t => t.TallaUs)
                    .Select(t => new
                    {
                        id = t.Id,
                        productoId = t.ProductoId,
                        tallaUs = t.TallaUs,
                        stock = t.Stock
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();

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

        if (producto.Stock <= 0)
            return BadRequest("El stock debe ser mayor a 0.");

        if (producto.Tallas == null || !producto.Tallas.Any())
            return BadRequest("Debes asignar stock por tallas.");

        var sumaTallas = producto.Tallas.Sum(t => t.Stock);

        if (sumaTallas != producto.Stock)
            return BadRequest($"La suma de tallas debe ser igual al stock total. Stock: {producto.Stock}, tallas: {sumaTallas}");

        var tallasDuplicadas = producto.Tallas
            .GroupBy(t => t.TallaUs)
            .Any(g => g.Count() > 1);

        if (tallasDuplicadas)
            return BadRequest("No puedes repetir la misma talla.");

        producto.Id = 0;
        producto.Marca = null;

        producto.Tallas = producto.Tallas
            .Where(t => t.Stock > 0)
            .Select(t => new ProductoTalla
            {
                TallaUs = t.TallaUs,
                Stock = t.Stock
            })
            .ToList();

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

        var productoDb = await _context.Productos
            .Include(p => p.Tallas)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (productoDb == null)
            return NotFound("Producto no encontrado.");

        var marcaExiste = await _context.Marcas.AnyAsync(m => m.Id == producto.MarcaId);
        if (!marcaExiste)
            return BadRequest("La marca no existe.");

        if (producto.Stock <= 0)
            return BadRequest("El stock debe ser mayor a 0.");

        if (producto.Tallas == null || !producto.Tallas.Any())
            return BadRequest("Debes asignar stock por tallas.");

        var sumaTallas = producto.Tallas.Sum(t => t.Stock);

        if (sumaTallas != producto.Stock)
            return BadRequest($"La suma de tallas debe ser igual al stock total. Stock: {producto.Stock}, tallas: {sumaTallas}");

        var tallasDuplicadas = producto.Tallas
            .GroupBy(t => t.TallaUs)
            .Any(g => g.Count() > 1);

        if (tallasDuplicadas)
            return BadRequest("No puedes repetir la misma talla.");

        productoDb.Nombre = producto.Nombre;
        productoDb.Descripcion = producto.Descripcion;
        productoDb.Precio = producto.Precio;
        productoDb.Stock = producto.Stock;
        productoDb.ImagenUrl = producto.ImagenUrl;
        productoDb.Activo = producto.Activo;
        productoDb.MarcaId = producto.MarcaId;

        _context.ProductoTallas.RemoveRange(productoDb.Tallas);

        productoDb.Tallas = producto.Tallas
            .Where(t => t.Stock > 0)
            .Select(t => new ProductoTalla
            {
                ProductoId = productoDb.Id,
                TallaUs = t.TallaUs,
                Stock = t.Stock
            })
            .ToList();

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