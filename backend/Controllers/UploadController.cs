using Microsoft.AspNetCore.Mvc;

namespace Tour_Project.Controllers
{
    [Route("api/upload")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        [HttpPost("image")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            var path = Path.Combine("wwwroot/images", file.FileName);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Ok(file.FileName);
        }

        [HttpPost("audio")]
        public async Task<IActionResult> UploadAudio(IFormFile file)
        {
            var path = Path.Combine("wwwroot/audio", file.FileName);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Ok(file.FileName);
        }
    }
}