using System.ComponentModel.DataAnnotations;

namespace PlumitaGrisAPI.DTOs
{
    public class ActualizarEstadoProductoDTO
    {
        [Required]
        public bool Activo { get; set; }
    }
}