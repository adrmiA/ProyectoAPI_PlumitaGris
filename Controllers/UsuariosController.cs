using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlumitaGrisAPI.Data;
using PlumitaGrisAPI.DTOs;
using PlumitaGrisAPI.Models;

namespace PlumitaGrisAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly PlumitaGrisContext _context;

        public UsuariosController(PlumitaGrisContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Usuario>>> GetUsuarios()
        {
            return Ok(await _context.Usuarios.Include(u => u.Rol).ToListAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Usuario>> GetUsuario(int id)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.IdUsuario == id);

            if (usuario == null)
                return NotFound(new { mensaje = $"Usuario con id {id} no encontrado" });

            return Ok(usuario);
        }

        [HttpPost]
        public async Task<ActionResult<Usuario>> PostUsuario(UsuarioDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var correoExiste = await _context.Usuarios.AnyAsync(u => u.Correo == dto.Correo);
            if (correoExiste)
                return Conflict(new { mensaje = "Ya existe un usuario con ese correo" });

            var rolExiste = await _context.Roles.AnyAsync(r => r.IdRol == dto.IdRol);
            if (!rolExiste)
                return BadRequest(new { mensaje = "El rol especificado no existe" });

            var usuario = new Usuario
            {
                Nombre = dto.Nombre,
                Correo = dto.Correo,
                Contrasena = dto.Contrasena, // En producción: hashear la contraseña
                IdRol = dto.IdRol,
                FechaRegistro = DateTime.Now
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUsuario), new { id = usuario.IdUsuario }, usuario);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutUsuario(int id, UsuarioDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
                return NotFound(new { mensaje = $"Usuario con id {id} no encontrado" });

            var rolExiste = await _context.Roles.AnyAsync(r => r.IdRol == dto.IdRol);
            if (!rolExiste)
                return BadRequest(new { mensaje = "El rol especificado no existe" });

            usuario.Nombre = dto.Nombre;
            usuario.Correo = dto.Correo;
            usuario.IdRol = dto.IdRol;

            await _context.SaveChangesAsync();
            return Ok(usuario);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
                return NotFound(new { mensaje = $"Usuario con id {id} no encontrado" });

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}