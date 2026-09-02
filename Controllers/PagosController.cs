using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlumitaGrisAPI.Data;
using PlumitaGrisAPI.DTOs;
using PlumitaGrisAPI.Models;

namespace PlumitaGrisAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PagosController : ControllerBase
    {
        private readonly PlumitaGrisContext _context;

        public PagosController(PlumitaGrisContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Pago>>> GetPagos()
        {
            return Ok(await _context.Pagos.Include(p => p.Pedido).ToListAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Pago>> GetPago(int id)
        {
            var pago = await _context.Pagos.Include(p => p.Pedido)
                .FirstOrDefaultAsync(p => p.IdPago == id);

            if (pago == null)
                return NotFound(new { mensaje = $"Pago con id {id} no encontrado" });

            return Ok(pago);
        }

        [HttpPost]
        public async Task<ActionResult<Pago>> PostPago(PagoDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var pedido = await _context.Pedidos.FindAsync(dto.IdPedido);
            if (pedido == null)
                return BadRequest(new { mensaje = "El pedido especificado no existe" });

            var pagoExiste = await _context.Pagos.AnyAsync(p => p.IdPedido == dto.IdPedido);
            if (pagoExiste)
                return Conflict(new { mensaje = "Este pedido ya tiene un pago registrado" });

            var pago = new Pago
            {
                IdPedido = dto.IdPedido,
                MetodoPago = dto.MetodoPago,
                Monto = dto.Monto,
                EstadoPago = "PENDIENTE",
                FechaPago = DateTime.Now
            };

            _context.Pagos.Add(pago);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPago), new { id = pago.IdPago }, pago);
        }

        // PUT: api/pagos/5/estado  → aprobar o rechazar el pago
        [HttpPut("{id}/estado")]
        public async Task<IActionResult> ActualizarEstadoPago(int id, [FromBody] string estado)
        {
            var estadosValidos = new[] { "PENDIENTE", "APROBADO", "RECHAZADO" };
            if (!estadosValidos.Contains(estado))
                return BadRequest(new { mensaje = "Estado no válido", estadosPermitidos = estadosValidos });

            var pago = await _context.Pagos.FindAsync(id);
            if (pago == null)
                return NotFound(new { mensaje = $"Pago con id {id} no encontrado" });

            pago.EstadoPago = estado;
            await _context.SaveChangesAsync();

            // Si se aprueba, actualizamos el estado del pedido
            if (estado == "APROBADO")
            {
                var pedido = await _context.Pedidos.FindAsync(pago.IdPedido);
                if (pedido != null)
                {
                    pedido.Estado = "PAGO_CONFIRMADO";
                    await _context.SaveChangesAsync();
                }
            }

            return Ok(pago);
        }
    }
}