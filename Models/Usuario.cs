using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlumitaGrisAPI.Models
{
    [Table("USUARIO")]
    public class Usuario
    {
        [Key]
        [Column("id_usuario")]
        public int IdUsuario { get; set; }

        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Column("correo")]
        public string Correo { get; set; } = string.Empty;

        [Column("contrasena")]
        public string Contrasena { get; set; } = string.Empty;

        [Column("id_rol")]
        public int IdRol { get; set; }

        [Column("fecha_registro")]
        public DateTime FechaRegistro { get; set; }

        public Rol? Rol { get; set; }
    }
}