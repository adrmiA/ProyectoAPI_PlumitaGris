using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlumitaGrisAPI.Data;

namespace PlumitaGrisAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ModalidadesEntregaController : ControllerBase
    {
        private readonly PlumitaGrisContext _context;

        public ModalidadesEntregaController(PlumitaGrisContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetModalidades()
        {
            return Ok(await _context.ModalidadesEntrega.ToListAsync());
        }
    }
}