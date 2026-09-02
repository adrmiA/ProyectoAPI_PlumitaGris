using System.ComponentModel.DataAnnotations;

namespace PlumitaGrisAPI.DTOs
{
    public class InventarioDTO
    {
        [Required]
        public int IdProducto { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "La cantidad no puede ser negativa")]
        public int CantidadDisponible { get; set; }
    }
}