using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlumitaGrisAPI.Models
{
    [Table("ADMINISTRADOR")]
    public class Administrador
    {
        [Key]
        [Column("id_administrador")]
        public int IdAdministrador { get; set; }

        [Column("id_usuario")]
        public int IdUsuario { get; set; }

        [Column("cargo")]
        public string? Cargo { get; set; }

        public Usuario? Usuario { get; set; }
    }
}