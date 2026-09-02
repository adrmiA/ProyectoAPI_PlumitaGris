using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlumitaGrisAPI.Models
{
    [Table("PEDIDO")]
    public class Pedido
    {
        [Key]
        [Column("id_pedido")]
        public int IdPedido { get; set; }

        [Column("id_cliente")]
        public int IdCliente { get; set; }

        [Column("fecha_pedido")]
        public DateTime FechaPedido { get; set; }

        [Column("modalidad_entrega")]
        public string ModalidadEntrega { get; set; } = "RECOGER_EN_LOCAL";

        [Column("estado")]
        public string Estado { get; set; } = "PENDIENTE_PAGO";

        public Cliente? Cliente { get; set; }
        public ICollection<DetallePedido>? Detalles { get; set; }
    }
}