using System.ComponentModel.DataAnnotations;

namespace PlumitaGrisAPI.DTOs
{
    public class PedidoDTO
    {
        [Required(ErrorMessage = "El cliente es obligatorio")]
        public int IdCliente { get; set; }

        [Required(ErrorMessage = "La modalidad de entrega es obligatoria")]
        public int IdModalidadEntrega { get; set; }

        [Required(ErrorMessage = "El pedido debe tener al menos un producto")]
        [MinLength(1)]
        public List<DetallePedidoDTO> Detalles { get; set; } = new();
    }

    public class DetallePedidoDTO
    {
        [Required]
        public int IdProducto { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        public int Cantidad { get; set; }
    }

    public class ActualizarEstadoDTO
    {
        [Required(ErrorMessage = "El estado es obligatorio")]
        public int IdEstadoPedido { get; set; }
    }
}