using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlumitaGrisAPI.Models
{
    [Table("AUDITORIA")]
    public class Auditoria
    {
        [Key]
        [Column("id_auditoria")]
        public int IdAuditoria { get; set; }

        [Column("id_administrador")]
        public int? IdAdministrador { get; set; }

        [Column("accion")]
        public string Accion { get; set; } = string.Empty;

        [Column("tabla_afectada")]
        public string TablaAfectada { get; set; } = string.Empty;

        [Column("id_registro")]
        public int? IdRegistro { get; set; }

        [Column("fecha_hora")]
        public DateTime FechaHora { get; set; }

        [Column("descripcion")]
        public string? Descripcion { get; set; }
    }
}