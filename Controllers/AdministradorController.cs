using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlumitaGrisAPI.Data;
using PlumitaGrisAPI.DTOs;
using PlumitaGrisAPI.Models;

namespace PlumitaGrisAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdministradorController : ControllerBase
    {
        private readonly PlumitaGrisContext _context;

        public AdministradorController(PlumitaGrisContext context)
        {
            _context = context;
        }

        // GET: api/administrador
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Administrador>>> GetAdministradores()
        {
            return Ok(await _context.Administradores
                .Include(a => a.Usuario)
                .ToListAsync());
        }

        // GET: api/administrador/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Administrador>> GetAdministrador(int id)
        {
            var administrador = await _context.Administradores
                .Include(a => a.Usuario)
                .FirstOrDefaultAsync(a => a.IdAdministrador == id);

            if (administrador == null)
                return NotFound(new { mensaje = $"Administrador con id {id} no encontrado" });

            return Ok(administrador);
        }

        // POST: api/administrador
        [HttpPost]
        public async Task<ActionResult<Administrador>> PostAdministrador(AdministradorDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var usuario = await _context.Usuarios.FindAsync(dto.IdUsuario);
            if (usuario == null)
                return BadRequest(new { mensaje = "El usuario especificado no existe" });

            var idRolAdmin = await _context.Roles
    .Where(r => r.Nombre == "ADMINISTRADOR")
    .Select(r => r.IdRol)
    .FirstOrDefaultAsync();

            if (usuario.IdRol != idRolAdmin)
                return BadRequest(new { mensaje = "El usuario debe tener rol ADMINISTRADOR para asignarse a esta tabla" });

            var yaEsAdmin = await _context.Administradores.AnyAsync(a => a.IdUsuario == dto.IdUsuario);
            if (yaEsAdmin)
                return Conflict(new { mensaje = "Este usuario ya está registrado como administrador" });

            var administrador = new Administrador
            {
                IdUsuario = dto.IdUsuario,
                Cargo = dto.Cargo
            };

            _context.Administradores.Add(administrador);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAdministrador), new { id = administrador.IdAdministrador }, administrador);
        }

        // PUT: api/administrador/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAdministrador(int id, AdministradorDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var administrador = await _context.Administradores.FindAsync(id);
            if (administrador == null)
                return NotFound(new { mensaje = $"Administrador con id {id} no encontrado" });

            administrador.Cargo = dto.Cargo;

            await _context.SaveChangesAsync();
            return Ok(administrador);
        }

        // DELETE: api/administrador/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAdministrador(int id)
        {
            var administrador = await _context.Administradores.FindAsync(id);
            if (administrador == null)
                return NotFound(new { mensaje = $"Administrador con id {id} no encontrado" });

            try
            {
                _context.Administradores.Remove(administrador);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateException)
            {
                // fk_auditoria_administrador tiene ON DELETE SET NULL, así que normalmente
                // esto no debería fallar, pero se captura por seguridad
                return Conflict(new { mensaje = "No se pudo eliminar el administrador" });
            }
        }
    }
}