using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlumitaGrisAPI.Data;

namespace PlumitaGrisAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EstadosPedidoController : ControllerBase
    {
        private readonly PlumitaGrisContext _context;

        public EstadosPedidoController(PlumitaGrisContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetEstadosPedido()
        {
            return Ok(await _context.EstadosPedido.ToListAsync());
        }
    }
}