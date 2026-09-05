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
        private readonly IWebHostEnvironment _environment;

        private static readonly string[] ExtensionesPermitidas = { ".jpg", ".jpeg", ".png", ".webp" };
        private const long TamanioMaximoBytes = 5 * 1024 * 1024; // 5 MB

        public ProductosController(PlumitaGrisContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
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
                    IdCategoria = dto.IdCategoria,
                    Activo = true
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

        // PUT: api/productos/5/estado
        // Activa o desactiva un producto sin borrarlo. Útil para productos que
        // ya tienen pedidos asociados y no se pueden eliminar por integridad referencial:
        // se desactivan para que dejen de mostrarse (p. ej. en la app móvil del cliente),
        // pero el historial de pedidos se conserva intacto.
        [HttpPut("{id}/estado")]
        public async Task<IActionResult> ActualizarEstadoProducto(int id, ActualizarEstadoProductoDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
                return NotFound(new { mensaje = $"Producto con id {id} no encontrado" });

            producto.Activo = dto.Activo;
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

            var tienePedidos = await _context.DetallesPedido.AnyAsync(dp => dp.IdProducto == id);
            if (tienePedidos)
                return Conflict(new { mensaje = "No se puede eliminar: el producto tiene pedidos asociados" });

            try
            {
                var inventario = await _context.Inventarios.FirstOrDefaultAsync(i => i.IdProducto == id);
                if (inventario != null)
                    _context.Inventarios.Remove(inventario);

                var detallesCarrito = _context.DetallesCarrito.Where(dc => dc.IdProducto == id);
                _context.DetallesCarrito.RemoveRange(detallesCarrito);

                _context.Productos.Remove(producto);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                return Conflict(new { mensaje = "No se puede eliminar el producto", detalle = ex.InnerException?.Message ?? ex.Message });
            }
        }

        // POST: api/productos/5/imagen
        [HttpPost("{id}/imagen")]
        [RequestSizeLimit(TamanioMaximoBytes)]
        public async Task<ActionResult> SubirImagen(int id, IFormFile archivo)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
                return NotFound(new { mensaje = $"Producto con id {id} no encontrado" });

            if (archivo == null || archivo.Length == 0)
                return BadRequest(new { mensaje = "No se envió ningún archivo" });

            if (archivo.Length > TamanioMaximoBytes)
                return BadRequest(new { mensaje = "El archivo supera el tamaño máximo permitido (5 MB)" });

            var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
            if (!ExtensionesPermitidas.Contains(extension))
                return BadRequest(new { mensaje = "Formato de imagen no permitido. Usa jpg, jpeg, png o webp" });

            try
            {
                var carpetaImagenes = Path.Combine(_environment.WebRootPath, "imagenes", "productos");
                Directory.CreateDirectory(carpetaImagenes);

                var nombreArchivo = $"producto_{id}_{Guid.NewGuid()}{extension}";
                var rutaFisica = Path.Combine(carpetaImagenes, nombreArchivo);

                if (!string.IsNullOrEmpty(producto.ImagenUrl))
                {
                    var rutaAnterior = Path.Combine(_environment.WebRootPath,
                        producto.ImagenUrl.TrimStart('/'));
                    if (System.IO.File.Exists(rutaAnterior))
                        System.IO.File.Delete(rutaAnterior);
                }

                using (var stream = new FileStream(rutaFisica, FileMode.Create))
                {
                    await archivo.CopyToAsync(stream);
                }

                // URL relativa que se guarda en la base de datos
                producto.ImagenUrl = $"/imagenes/productos/{nombreArchivo}";
                await _context.SaveChangesAsync();

                return Ok(new { mensaje = "Imagen subida correctamente", imagenUrl = producto.ImagenUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al subir la imagen", detalle = ex.Message });
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
                        p.Descripcion,
                        Categoria = new
                        {
                            IdCategoria = p.IdCategoria,
                            Nombre = p.Categoria!.Nombre
                        },
                        IdCategoria = p.IdCategoria,
                        p.Precio,
                        p.ImagenUrl,
                        p.Activo,
                        i.CantidadDisponible
                    })
                .Where(x => x.CantidadDisponible > 0 && x.Activo)
                .ToListAsync();

            return Ok(productos);
        }
    }
}