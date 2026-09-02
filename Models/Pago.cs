using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlumitaGrisAPI.Models
{
    [Table("PAGO")]
    public class Pago
    {
        [Key]
        [Column("id_pago")]
        public int IdPago { get; set; }

        [Column("id_pedido")]
        public int IdPedido { get; set; }

        [Column("metodo_pago")]
        public string MetodoPago { get; set; } = string.Empty;

        [Column("monto")]
        public decimal Monto { get; set; }

        [Column("id_estado_pago")]
        public int IdEstadoPago { get; set; }

        [Column("fecha_pago")]
        public DateTime FechaPago { get; set; }

        public Pedido? Pedido { get; set; }
        public EstadoPago? EstadoPago { get; set; }
    }
}