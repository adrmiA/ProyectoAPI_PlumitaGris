using System.ComponentModel.DataAnnotations;

namespace PlumitaGrisAPI.DTOs
{
    public class ClienteDTO
    {
        [Required]
        public int IdUsuario { get; set; }

        [StringLength(255)]
        public string? Direccion { get; set; }

        [StringLength(20)]
        public string? Telefono { get; set; }
    }
}