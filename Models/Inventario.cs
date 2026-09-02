using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlumitaGrisAPI.Models
{
    [Table("INVENTARIO")]
    public class Inventario
    {
        [Key]
        [Column("id_inventario")]
        public int IdInventario { get; set; }

        [Column("id_producto")]
        public int IdProducto { get; set; }

        [Column("cantidad_disponible")]
        public int CantidadDisponible { get; set; }

        public Producto? Producto { get; set; }
    }
}