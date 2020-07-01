using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AquaMonitor.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AquaMonitor.Web.Controllers
{
    /// <summary>
    /// Camera Image Controller
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class CameraController : ControllerBase
    {

        private readonly ILogger<CameraController> logger;
        private readonly IGlobalState globalConfig;
        
        /// <summary>
        /// CTor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="globalConfig"></param>
        public CameraController(ILogger<CameraController> logger, IGlobalState globalConfig)
        {
            this.logger = logger;
            this.globalConfig = globalConfig;
        }

        /// <summary>
        /// Gets the most recent camera image
        /// </summary>
        /// <returns>JPG</returns>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            if (string.IsNullOrEmpty(globalConfig.More.CameraJPGUrl))
            {
                return this.NotFound();
            }

            var img = new byte[]{};
            try
            {
                string path = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    $"wwwroot/img/camera");
                var cameraFolder = new System.IO.DirectoryInfo(path);
                string file = cameraFolder.GetFiles().OrderByDescending(t => t.CreationTime).First().FullName;
                img = await System.IO.File.ReadAllBytesAsync(file);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to read camera image from disk: "+ ex.Message);
                return this.NoContent();
            }
            return File(img, "image/jpg", $"camerastream{Guid.NewGuid().ToString()}.jpg");
        }
    }
}
