using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlumitaGrisAPI.Models
{
    [Table("ESTADO_PAGO")]
    public class EstadoPago
    {
        [Key]
        [Column("id_estado_pago")]
        public int IdEstadoPago { get; set; }

        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;
    }
}