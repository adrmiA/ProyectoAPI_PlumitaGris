using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlumitaGrisAPI.Data;
using PlumitaGrisAPI.Models;

namespace PlumitaGrisAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CarritoController : ControllerBase
    {
        private readonly PlumitaGrisContext _context;

        public CarritoController(PlumitaGrisContext context)
        {
            _context = context;
        }

        // GET: api/carrito/cliente/5
        [HttpGet("cliente/{idCliente}")]
        public async Task<ActionResult> GetCarritoPorCliente(int idCliente)
        {
            var carrito = await _context.Carritos
                .FirstOrDefaultAsync(c => c.IdCliente == idCliente);

            if (carrito == null)
                return NotFound(new { mensaje = "Este cliente no tiene carrito activo" });

            var detalles = await _context.DetallesCarrito
                .Include(d => d.Producto)
                .Where(d => d.IdCarrito == carrito.IdCarrito)
                .ToListAsync();

            return Ok(new { carrito.IdCarrito, carrito.FechaCreacion, Productos = detalles });
        }

        // POST: api/carrito
        [HttpPost]
        public async Task<ActionResult<Carrito>> CrearCarrito([FromBody] int idCliente)
        {
            var existente = await _context.Carritos.AnyAsync(c => c.IdCliente == idCliente);
            if (existente)
                return Conflict(new { mensaje = "El cliente ya tiene un carrito" });

            var carrito = new Carrito { IdCliente = idCliente, FechaCreacion = DateTime.Now };
            _context.Carritos.Add(carrito);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCarritoPorCliente), new { idCliente }, carrito);
        }

        // POST: api/carrito/5/productos
        [HttpPost("{idCarrito}/productos")]
        public async Task<IActionResult> AgregarProducto(int idCarrito, [FromBody] DetalleCarrito detalle)
        {
            var carritoExiste = await _context.Carritos.AnyAsync(c => c.IdCarrito == idCarrito);
            if (!carritoExiste)
                return NotFound(new { mensaje = "Carrito no encontrado" });

            var productoExiste = await _context.Productos.AnyAsync(p => p.IdProducto == detalle.IdProducto);
            if (!productoExiste)
                return BadRequest(new { mensaje = "Producto no encontrado" });

            if (detalle.Cantidad <= 0)
                return BadRequest(new { mensaje = "La cantidad debe ser mayor a 0" });

            detalle.IdCarrito = idCarrito;
            _context.DetallesCarrito.Add(detalle);
            await _context.SaveChangesAsync();

            return Ok(detalle);
        }

        // DELETE: api/carrito/detalle/5
        [HttpDelete("detalle/{idDetalle}")]
        public async Task<IActionResult> EliminarProducto(int idDetalle)
        {
            var detalle = await _context.DetallesCarrito.FindAsync(idDetalle);
            if (detalle == null)
                return NotFound(new { mensaje = "Detalle no encontrado" });

            _context.DetallesCarrito.Remove(detalle);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}