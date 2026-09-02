using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlumitaGrisAPI.Models
{
    [Table("CARRITO")]
    public class Carrito
    {
        [Key]
        [Column("id_carrito")]
        public int IdCarrito { get; set; }

        [Column("id_cliente")]
        public int IdCliente { get; set; }

        [Column("fecha_creacion")]
        public DateTime FechaCreacion { get; set; }

        public Cliente? Cliente { get; set; }
    }
}