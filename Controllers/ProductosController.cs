using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlumitaGrisAPI.Data;
using PlumitaGrisAPI.DTOs;
using PlumitaGrisAPI.Models;

namespace PlumitaGrisAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductosController : ControllerBase
    {
        private readonly PlumitaGrisContext _context;

        public ProductosController(PlumitaGrisContext context)
        {
            _context = context;
        }

        // GET: api/productos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Producto>>> GetProductos()
        {
            return Ok(await _context.Productos
                .Include(p => p.Categoria)
                .ToListAsync());
        }

        // GET: api/productos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Producto>> GetProducto(int id)
        {
            var producto = await _context.Productos
                .Include(p => p.Categoria)
                .FirstOrDefaultAsync(p => p.IdProducto == id);

            if (producto == null)
                return NotFound(new { mensaje = $"Producto con id {id} no encontrado" });

            return Ok(producto);
        }

        // POST: api/productos
        [HttpPost]
        public async Task<ActionResult<Producto>> PostProducto(ProductoDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var categoriaExiste = await _context.Categorias.AnyAsync(c => c.IdCategoria == dto.IdCategoria);
            if (!categoriaExiste)
                return BadRequest(new { mensaje = "La categoría especificada no existe" });

            try
            {
                var producto = new Producto
                {
                    Nombre = dto.Nombre,
                    Descripcion = dto.Descripcion,
                    Precio = dto.Precio,
                    IdCategoria = dto.IdCategoria
                };

                _context.Productos.Add(producto);
                await _context.SaveChangesAsync();

                // Crear registro de inventario en 0 automáticamente
                _context.Inventarios.Add(new Inventario
                {
                    IdProducto = producto.IdProducto,
                    CantidadDisponible = 0
                });
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetProducto), new { id = producto.IdProducto }, producto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al crear el producto", detalle = ex.Message });
            }
        }

        // PUT: api/productos/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProducto(int id, ProductoDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
                return NotFound(new { mensaje = $"Producto con id {id} no encontrado" });

            var categoriaExiste = await _context.Categorias.AnyAsync(c => c.IdCategoria == dto.IdCategoria);
            if (!categoriaExiste)
                return BadRequest(new { mensaje = "La categoría especificada no existe" });

            producto.Nombre = dto.Nombre;
            producto.Descripcion = dto.Descripcion;
            producto.Precio = dto.Precio;
            producto.IdCategoria = dto.IdCategoria;

            await _context.SaveChangesAsync();
            return Ok(producto);
        }

        // DELETE: api/productos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProducto(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
                return NotFound(new { mensaje = $"Producto con id {id} no encontrado" });

            try
            {
                _context.Productos.Remove(producto);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateException)
            {
                return Conflict(new { mensaje = "No se puede eliminar: el producto tiene pedidos asociados" });
            }
        }

        // GET: api/productos/disponibles
        [HttpGet("disponibles")]
        public async Task<ActionResult> GetProductosDisponibles()
        {
            var productos = await _context.Productos
                .Include(p => p.Categoria)
                .Join(_context.Inventarios, p => p.IdProducto, i => i.IdProducto,
                    (p, i) => new
                    {
                        p.IdProducto,
                        p.Nombre,
                        Categoria = p.Categoria!.Nombre,
                        p.Precio,
                        i.CantidadDisponible
                    })
                .Where(x => x.CantidadDisponible > 0)
                .ToListAsync();

            return Ok(productos);
        }
    }
}