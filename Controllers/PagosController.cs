using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlumitaGrisAPI.Data;
using PlumitaGrisAPI.DTOs;
using PlumitaGrisAPI.Models;
using PlumitaGrisAPI.Services;

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
                var pedido = await _context.Pedidos
                    .Include(p => p.EstadoPedido)
                    .FirstOrDefaultAsync(p => p.IdPedido == pago.IdPedido);

                if (pedido != null)
                {
                    var nuevoEstado = await _context.EstadosPedido
                        .FirstOrDefaultAsync(e => e.Nombre == "EN PREPARACION");

                    if (nuevoEstado != null)
                    {
                        var estadoAnteriorNombre = pedido.EstadoPedido?.Nombre;

                        using var transaccion = await _context.Database.BeginTransactionAsync();
                        try
                        {
                            var errorStock = await InventarioService.AjustarStockPorCambioEstado(
                                _context, pedido.IdPedido, estadoAnteriorNombre, nuevoEstado.Nombre);

                            if (errorStock == null)
                            {
                                pedido.IdEstadoPedido = nuevoEstado.IdEstadoPedido;
                                await _context.SaveChangesAsync();
                                await transaccion.CommitAsync();
                            }
                            else
                            {
                                await transaccion.RollbackAsync();
                                return Conflict(new { mensaje = errorStock });
                            }
                        }
                        catch (Exception)
                        {
                            await transaccion.RollbackAsync();
                            throw;
                        }
                    }
                }
            }

            return Ok(pago);
        }
    }
}