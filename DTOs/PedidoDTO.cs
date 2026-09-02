using System.ComponentModel.DataAnnotations;

namespace PlumitaGrisAPI.DTOs
{
    public class PedidoDTO
    {
        [Required(ErrorMessage = "El cliente es obligatorio")]
        public int IdCliente { get; set; }

        [Required]
        [RegularExpression("RECOGER_EN_LOCAL|DOMICILIO", ErrorMessage = "Modalidad no válida")]
        public string ModalidadEntrega { get; set; } = "RECOGER_EN_LOCAL";

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
        [Required]
        public string Estado { get; set; } = string.Empty;
    }
}