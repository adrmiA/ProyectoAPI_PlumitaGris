using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlumitaGrisAPI.Data;

namespace PlumitaGrisAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RolesController : ControllerBase
    {
        private readonly PlumitaGrisContext _context;

        public RolesController(PlumitaGrisContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetRoles()
        {
            return Ok(await _context.Roles.ToListAsync());
        }
    }
}