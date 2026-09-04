using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlumitaGrisAPI.Data;

namespace PlumitaGrisAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuditoriaController : ControllerBase
    {
        private readonly PlumitaGrisContext _context;

        public AuditoriaController(PlumitaGrisContext context)
        {
            _context = context;
        }

        // GET: api/auditoria
        [HttpGet]
        public async Task<ActionResult> GetAuditoria()
        {
            var auditoria = await _context.Auditorias
                .OrderByDescending(a => a.FechaHora)
                .ToListAsync();

            return Ok(auditoria);
        }

        // GET: api/auditoria/5
        [HttpGet("{id}")]
        public async Task<ActionResult> GetRegistro(int id)
        {
            var registro = await _context.Auditorias.FindAsync(id);
            if (registro == null)
                return NotFound(new { mensaje = $"Registro de auditoría con id {id} no encontrado" });

            return Ok(registro);
        }

        // GET: api/auditoria/tabla/PRODUCTO
        [HttpGet("tabla/{tablaAfectada}")]
        public async Task<ActionResult> GetPorTabla(string tablaAfectada)
        {
            var registros = await _context.Auditorias
                .Where(a => a.TablaAfectada.ToUpper() == tablaAfectada.ToUpper())
                .OrderByDescending(a => a.FechaHora)
                .ToListAsync();

            if (!registros.Any())
                return NotFound(new { mensaje = $"No hay registros de auditoría para la tabla {tablaAfectada}" });

            return Ok(registros);
        }

        // GET: api/auditoria/administrador/5
        [HttpGet("administrador/{idAdministrador}")]
        public async Task<ActionResult> GetPorAdministrador(int idAdministrador)
        {
            var registros = await _context.Auditorias
                .Where(a => a.IdAdministrador == idAdministrador)
                .OrderByDescending(a => a.FechaHora)
                .ToListAsync();

            return Ok(registros);
        }

        // GET: api/auditoria/rango?desde=2026-01-01&hasta=2026-12-31
        [HttpGet("rango")]
        public async Task<ActionResult> GetPorRangoFechas([FromQuery] DateTime desde, [FromQuery] DateTime hasta)
        {
            if (desde > hasta)
                return BadRequest(new { mensaje = "La fecha 'desde' no puede ser mayor que 'hasta'" });

            var registros = await _context.Auditorias
                .Where(a => a.FechaHora >= desde && a.FechaHora <= hasta)
                .OrderByDescending(a => a.FechaHora)
                .ToListAsync();

            return Ok(registros);
        }

    }
}