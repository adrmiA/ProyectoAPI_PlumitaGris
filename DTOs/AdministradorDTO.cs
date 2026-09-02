using System.ComponentModel.DataAnnotations;

namespace PlumitaGrisAPI.DTOs
{
    public class AdministradorDTO
    {
        [Required(ErrorMessage = "El usuario es obligatorio")]
        public int IdUsuario { get; set; }

        [StringLength(100)]
        public string? Cargo { get; set; }
    }
}