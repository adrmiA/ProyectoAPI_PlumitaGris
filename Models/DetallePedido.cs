using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlumitaGrisAPI.Models
{
    [Table("DETALLEPEDIDO")]
    public class DetallePedido
    {
        [Key]
        [Column("id_detalle_pedido")]
        public int IdDetallePedido { get; set; }

        [Column("id_pedido")]
        public int IdPedido { get; set; }

        [Column("id_producto")]
        public int IdProducto { get; set; }

        [Column("cantidad")]
        public int Cantidad { get; set; }

        [Column("subtotal")]
        public decimal Subtotal { get; set; }

        public Pedido? Pedido { get; set; }
        public Producto? Producto { get; set; }
    }
}