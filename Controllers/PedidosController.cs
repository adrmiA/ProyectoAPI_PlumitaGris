using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlumitaGrisAPI.Data;
using PlumitaGrisAPI.DTOs;
using PlumitaGrisAPI.Models;

namespace PlumitaGrisAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PedidosController : ControllerBase
    {
        private readonly PlumitaGrisContext _context;

        public PedidosController(PlumitaGrisContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Pedido>>> GetPedidos()
        {
            return Ok(await _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.EstadoPedido)
                .Include(p => p.ModalidadEntrega)
                .Include(p => p.Detalles)
                .ThenInclude(d => d.Producto)
                .ToListAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Pedido>> GetPedido(int id)
        {
            var pedido = await _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.EstadoPedido)
                .Include(p => p.ModalidadEntrega)
                .Include(p => p.Detalles)
                .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(p => p.IdPedido == id);

            if (pedido == null)
                return NotFound(new { mensaje = $"Pedido con id {id} no encontrado" });

            return Ok(pedido);
        }

        [HttpPost]
        public async Task<ActionResult<Pedido>> PostPedido(PedidoDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var clienteExiste = await _context.Clientes.AnyAsync(c => c.IdCliente == dto.IdCliente);
            if (!clienteExiste)
                return BadRequest(new { mensaje = "El cliente especificado no existe" });

            var modalidadExiste = await _context.ModalidadesEntrega.AnyAsync(m => m.IdModalidadEntrega == dto.IdModalidadEntrega);
            if (!modalidadExiste)
                return BadRequest(new { mensaje = "La modalidad de entrega especificada no existe" });

            var idEstadoInicial = await _context.EstadosPedido
                .Where(e => e.Nombre == "PENDIENTE_PAGO")
                .Select(e => e.IdEstadoPedido)
                .FirstOrDefaultAsync();

            using var transaccion = await _context.Database.BeginTransactionAsync();
            try
            {
                var pedido = new Pedido
                {
                    IdCliente = dto.IdCliente,
                    FechaPedido = DateTime.Now,
                    IdModalidadEntrega = dto.IdModalidadEntrega,
                    IdEstadoPedido = idEstadoInicial
                };

                _context.Pedidos.Add(pedido);
                await _context.SaveChangesAsync();

                decimal totalPedido = 0;

                foreach (var det in dto.Detalles)
                {
                    var producto = await _context.Productos.FindAsync(det.IdProducto);
                    if (producto == null)
                        return BadRequest(new { mensaje = $"El producto con id {det.IdProducto} no existe" });

                    var inventario = await _context.Inventarios
                        .FirstOrDefaultAsync(i => i.IdProducto == det.IdProducto);

                    if (inventario == null || inventario.CantidadDisponible < det.Cantidad)
                        return BadRequest(new { mensaje = $"Stock insuficiente para el producto {producto.Nombre}" });

                    var subtotal = producto.Precio * det.Cantidad;
                    totalPedido += subtotal;

                    _context.DetallesPedido.Add(new DetallePedido
                    {
                        IdPedido = pedido.IdPedido,
                        IdProducto = det.IdProducto,
                        Cantidad = det.Cantidad,
                        Subtotal = subtotal
                    });
                }

                await _context.SaveChangesAsync();
                await transaccion.CommitAsync();

                return CreatedAtAction(nameof(GetPedido), new { id = pedido.IdPedido },
                    new { pedido.IdPedido, pedido.IdEstadoPedido, Total = totalPedido });
            }
            catch (Exception ex)
            {
                await transaccion.RollbackAsync();
                return StatusCode(500, new { mensaje = "Error al crear el pedido", detalle = ex.Message });
            }
        }

        // PUT: api/pedidos/5/estado
        [HttpPut("{id}/estado")]
        public async Task<IActionResult> ActualizarEstado(int id, ActualizarEstadoDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var estadoExiste = await _context.EstadosPedido.AnyAsync(e => e.IdEstadoPedido == dto.IdEstadoPedido);
            if (!estadoExiste)
                return BadRequest(new { mensaje = "El estado especificado no existe" });

            var pedido = await _context.Pedidos.FindAsync(id);
            if (pedido == null)
                return NotFound(new { mensaje = $"Pedido con id {id} no encontrado" });

            pedido.IdEstadoPedido = dto.IdEstadoPedido; // Dispara TR_ActualizarInventario / TR_RestaurarInventario / TR_AuditoriaPedido
            await _context.SaveChangesAsync();

            return Ok(pedido);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePedido(int id)
        {
            var pedido = await _context.Pedidos.FindAsync(id);
            if (pedido == null)
                return NotFound(new { mensaje = $"Pedido con id {id} no encontrado" });

            try
            {
                _context.Pedidos.Remove(pedido);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateException)
            {
                return Conflict(new { mensaje = "No se puede eliminar: el pedido tiene pagos asociados" });
            }
        }
    }
}