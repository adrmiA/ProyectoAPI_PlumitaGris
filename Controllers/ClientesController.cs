using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlumitaGrisAPI.Data;
using PlumitaGrisAPI.DTOs;
using PlumitaGrisAPI.Models;

namespace PlumitaGrisAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClientesController : ControllerBase
    {
        private readonly PlumitaGrisContext _context;

        public ClientesController(PlumitaGrisContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Cliente>>> GetClientes()
        {
            return Ok(await _context.Clientes.Include(c => c.Usuario).ToListAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Cliente>> GetCliente(int id)
        {
            var cliente = await _context.Clientes
                .Include(c => c.Usuario)
                .FirstOrDefaultAsync(c => c.IdCliente == id);

            if (cliente == null)
                return NotFound(new { mensaje = $"Cliente con id {id} no encontrado" });

            return Ok(cliente);
        }

        [HttpPost]
        public async Task<ActionResult<Cliente>> PostCliente(ClienteDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var usuarioExiste = await _context.Usuarios.AnyAsync(u => u.IdUsuario == dto.IdUsuario);
            if (!usuarioExiste)
                return BadRequest(new { mensaje = "El usuario especificado no existe" });

            var yaEsCliente = await _context.Clientes.AnyAsync(c => c.IdUsuario == dto.IdUsuario);
            if (yaEsCliente)
                return Conflict(new { mensaje = "Este usuario ya está registrado como cliente" });

            var cliente = new Cliente
            {
                IdUsuario = dto.IdUsuario,
                Direccion = dto.Direccion,
                Telefono = dto.Telefono
            };

            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCliente), new { id = cliente.IdCliente }, cliente);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutCliente(int id, ClienteDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null)
                return NotFound(new { mensaje = $"Cliente con id {id} no encontrado" });

            cliente.Direccion = dto.Direccion;
            cliente.Telefono = dto.Telefono;

            await _context.SaveChangesAsync();
            return Ok(cliente);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCliente(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null)
                return NotFound(new { mensaje = $"Cliente con id {id} no encontrado" });

            _context.Clientes.Remove(cliente);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}