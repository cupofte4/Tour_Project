using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using backend.Services;
using backend.Application.Location;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LocationsController : ControllerBase
    {
        private readonly ILocationService _svc;

        public LocationsController(ILocationService svc)
        {
            _svc = svc;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _svc.GetAllAsync();
            return Ok(list);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var item = await _svc.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateLocationRequest req)
        {
            var item = await _svc.CreateAsync(req);
            return CreatedAtAction(nameof(Get), new { id = item.Id }, item);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateLocationRequest req)
        {
            var item = await _svc.UpdateAsync(id, req);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _svc.DeleteAsync(id);
            if (!ok) return NotFound();
            return NoContent();
        }
    }
}
