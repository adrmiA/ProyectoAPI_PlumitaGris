using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlumitaGrisAPI.Models
{
    [Table("ESTADO_PEDIDO")]
    public class EstadoPedido
    {
        [Key]
        [Column("id_estado_pedido")]
        public int IdEstadoPedido { get; set; }

        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;
    }
}