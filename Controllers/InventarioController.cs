using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlumitaGrisAPI.Data;
using PlumitaGrisAPI.DTOs;

namespace PlumitaGrisAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventarioController : ControllerBase
    {
        private readonly PlumitaGrisContext _context;

        public InventarioController(PlumitaGrisContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult> GetInventario()
        {
            var inventario = await _context.Inventarios
                .Include(i => i.Producto)
                .ThenInclude(p => p!.Categoria)
                .ToListAsync();

            return Ok(inventario);
        }

        [HttpGet("{idProducto}")]
        public async Task<ActionResult> GetInventarioPorProducto(int idProducto)
        {
            var inventario = await _context.Inventarios
                .Include(i => i.Producto)
                .FirstOrDefaultAsync(i => i.IdProducto == idProducto);

            if (inventario == null)
                return NotFound(new { mensaje = "No se encontró inventario para este producto" });

            return Ok(inventario);
        }

        [HttpPut("{idProducto}")]
        public async Task<IActionResult> ActualizarInventario(int idProducto, InventarioDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var inventario = await _context.Inventarios.FirstOrDefaultAsync(i => i.IdProducto == idProducto);
            if (inventario == null)
                return NotFound(new { mensaje = "No se encontró inventario para este producto" });

            inventario.CantidadDisponible = dto.CantidadDisponible;
            await _context.SaveChangesAsync();

            return Ok(inventario);
        }
    }
}