using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlumitaGrisAPI.Models
{
    [Table("MODALIDAD_ENTREGA")]
    public class ModalidadEntrega
    {
        [Key]
        [Column("id_modalidad_entrega")]
        public int IdModalidadEntrega { get; set; }

        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;
    }
}