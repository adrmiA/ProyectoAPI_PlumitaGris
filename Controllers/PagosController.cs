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
            return Ok(await _context.Pagos
                .Include(p => p.Pedido)
                .Include(p => p.EstadoPago)
                .ToListAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Pago>> GetPago(int id)
        {
            var pago = await _context.Pagos
                .Include(p => p.Pedido)
                .Include(p => p.EstadoPago)
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

            var idEstadoInicial = await _context.EstadosPago
                .Where(e => e.Nombre == "PENDIENTE")
                .Select(e => e.IdEstadoPago)
                .FirstOrDefaultAsync();

            var pago = new Pago
            {
                IdPedido = dto.IdPedido,
                MetodoPago = dto.MetodoPago,
                Monto = dto.Monto,
                IdEstadoPago = idEstadoInicial,
                FechaPago = DateTime.Now
            };

            _context.Pagos.Add(pago);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPago), new { id = pago.IdPago }, pago);
        }

        // PUT: api/pagos/5/estado
        [HttpPut("{id}/estado")]
        public async Task<IActionResult> ActualizarEstadoPago(int id, ActualizarEstadoPagoDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var estado = await _context.EstadosPago.FindAsync(dto.IdEstadoPago);
            if (estado == null)
                return BadRequest(new { mensaje = "El estado especificado no existe" });

            var pago = await _context.Pagos.FindAsync(id);
            if (pago == null)
                return NotFound(new { mensaje = $"Pago con id {id} no encontrado" });

            pago.IdEstadoPago = dto.IdEstadoPago;
            await _context.SaveChangesAsync();

            if (estado.Nombre == "APROBADO")
            {
                var pedido = await _context.Pedidos.FindAsync(pago.IdPedido);
                if (pedido != null)
                {
                    var idPagoConfirmado = await _context.EstadosPedido
                        .Where(e => e.Nombre == "PAGO_CONFIRMADO")
                        .Select(e => e.IdEstadoPedido)
                        .FirstOrDefaultAsync();

                    pedido.IdEstadoPedido = idPagoConfirmado;
                    await _context.SaveChangesAsync();
                }
            }

            return Ok(pago);
        }
    }
}