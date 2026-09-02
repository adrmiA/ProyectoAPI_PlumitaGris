using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlumitaGrisAPI.Data;

namespace PlumitaGrisAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EstadosPagoController : ControllerBase
    {
        private readonly PlumitaGrisContext _context;

        public EstadosPagoController(PlumitaGrisContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetEstadosPago()
        {
            return Ok(await _context.EstadosPago.ToListAsync());
        }
    }
}