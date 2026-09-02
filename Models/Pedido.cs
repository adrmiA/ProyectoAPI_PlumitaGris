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

        [Column("id_modalidad_entrega")]
        public int IdModalidadEntrega { get; set; }

        [Column("id_estado_pedido")]
        public int IdEstadoPedido { get; set; }

        public Cliente? Cliente { get; set; }
        public ModalidadEntrega? ModalidadEntrega { get; set; }
        public EstadoPedido? EstadoPedido { get; set; }
        public ICollection<DetallePedido>? Detalles { get; set; }
    }
}