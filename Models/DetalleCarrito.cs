using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlumitaGrisAPI.Models
{
    [Table("DETALLECARRITO")]
    public class DetalleCarrito
    {
        [Key]
        [Column("id_detalle_carrito")]
        public int IdDetalleCarrito { get; set; }

        [Column("id_carrito")]
        public int IdCarrito { get; set; }

        [Column("id_producto")]
        public int IdProducto { get; set; }

        [Column("cantidad")]
        public int Cantidad { get; set; }

        public Carrito? Carrito { get; set; }
        public Producto? Producto { get; set; }
    }
}