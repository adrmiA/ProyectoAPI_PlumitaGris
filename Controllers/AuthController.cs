using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlumitaGrisAPI.Data;
using PlumitaGrisAPI.DTOs;
using PlumitaGrisAPI.Utils;

namespace PlumitaGrisAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly PlumitaGrisContext _context;

        public AuthController(PlumitaGrisContext context)
        {
            _context = context;
        }

        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<ActionResult> Login(LoginDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Correo == dto.Correo);

            // Verificar que el usuario exista y que la contraseña sea correcta
            if (usuario == null ||
                !PasswordHasher.Verify(dto.Contrasena, usuario.Contrasena))
            {
                return Unauthorized(new
                {
                    mensaje = "Correo o contraseña incorrectos"
                });
            }

            // Verificar que tenga rol de ADMINISTRADOR
            if (usuario.Rol == null ||
                usuario.Rol.Nombre != "ADMINISTRADOR")
            {
                return Unauthorized(new
                {
                    mensaje = "No tienes permisos para acceder a esta aplicación"
                });
            }

            return Ok(new
            {
                idUsuario = usuario.IdUsuario,
                nombre = usuario.Nombre,
                correo = usuario.Correo,
                idRol = usuario.IdRol,
                rol = usuario.Rol.Nombre
            });
        }
    }
}