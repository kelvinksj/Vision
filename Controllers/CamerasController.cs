using Microsoft.AspNetCore.Mvc;
using CameraWebApp.Services;

namespace CameraWebApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CamerasController : ControllerBase
    {
        private readonly CameraService _cameraService;
        public CamerasController(CameraService cameraService)
        {
            _cameraService = cameraService;
        }

        [HttpGet]
        public IActionResult GetCameras()
        {
            var list = _cameraService.GetConnectedCameras();
            return Ok(list);
        }
    }
}
