using System;
using System.Threading.Tasks;
using AquaMonitor.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AquaMonitor.Web.Controllers
{
    /// <summary>
    /// Sky Image Controller
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class SkyController : ControllerBase
    {

        private readonly ILogger<SkyController> logger;
        private readonly SkyService skyService;

        /// <summary>
        /// CTor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="skyService"></param>
        public SkyController(ILogger<SkyController> logger, SkyService skyService)
        {
            this.logger = logger;
            this.skyService = skyService;
        }

        /// <summary>
        /// Gets the most recent camera image
        /// </summary>
        /// <returns>JPG</returns>
        [HttpGet]
        public async Task<IActionResult> Get(DateTime? start, DateTime? end)
        {
            var img = new byte[]{};
            try
            {
                img = skyService.BuildSkyBytes(start, end);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to render sky: "+ ex.Message);
                return this.NoContent();
            }
            return File(img, "image/png", $"sky{Guid.NewGuid().ToString()}.png");
        }
    }
}
