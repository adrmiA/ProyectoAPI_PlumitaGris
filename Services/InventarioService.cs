using Microsoft.EntityFrameworkCore;
using PlumitaGrisAPI.Data;

namespace PlumitaGrisAPI.Services
{
    public static class InventarioService
    {
        // El stock se descuenta al pasar a EN PREPARACION (momento en que se confirma el pago).
        // ENTREGADO también está en el conjunto para que, al pasar de EN PREPARACION a ENTREGADO,
        // no se interprete como una salida del estado "ya descontado" y se repongan por error.
        public static readonly HashSet<string> EstadosQueDescuentanStock = new()
        {
            "EN PREPARACION",
            "ENTREGADO"
        };

        public static async Task<string?> AjustarStockPorCambioEstado(
            PlumitaGrisContext context,
            int idPedido,
            string? estadoAnteriorNombre,
            string nuevoEstadoNombre)
        {
            bool eraDescontado = estadoAnteriorNombre != null && EstadosQueDescuentanStock.Contains(estadoAnteriorNombre);
            bool esDescontado = EstadosQueDescuentanStock.Contains(nuevoEstadoNombre);

            if (esDescontado == eraDescontado)
                return null;

            var detalles = await context.DetallesPedido
                .Where(d => d.IdPedido == idPedido)
                .ToListAsync();

            if (esDescontado && !eraDescontado)
            {
                foreach (var det in detalles)
                {
                    var inventario = await context.Inventarios
                        .FirstOrDefaultAsync(i => i.IdProducto == det.IdProducto);

                    if (inventario == null || inventario.CantidadDisponible < det.Cantidad)
                        return $"Stock insuficiente para descontar el producto con id {det.IdProducto}";
                }

                foreach (var det in detalles)
                {
                    var inventario = await context.Inventarios
                        .FirstAsync(i => i.IdProducto == det.IdProducto);
                    inventario.CantidadDisponible -= det.Cantidad;
                }
            }
            else if (!esDescontado && eraDescontado)
            {
                foreach (var det in detalles)
                {
                    var inventario = await context.Inventarios
                        .FirstOrDefaultAsync(i => i.IdProducto == det.IdProducto);
                    if (inventario != null)
                        inventario.CantidadDisponible += det.Cantidad;
                }
            }

            return null;
        }
    }
}