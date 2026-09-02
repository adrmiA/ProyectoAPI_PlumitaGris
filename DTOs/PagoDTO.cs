using System.ComponentModel.DataAnnotations;

namespace PlumitaGrisAPI.DTOs
{
    public class PagoDTO
    {
        [Required]
        public int IdPedido { get; set; }

        [Required(ErrorMessage = "El método de pago es obligatorio")]
        [StringLength(50)]
        public string MetodoPago { get; set; } = string.Empty;

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "El monto no puede ser negativo")]
        public decimal Monto { get; set; }
    }
}