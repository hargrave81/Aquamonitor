using System;
using System.Linq;
using AquaMonitor.Data.Models;
using AquaMonitor.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AquaMonitor.Web.Controllers
{
    /// <summary>
    /// Relay Controller
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    
    public class SystemController : ControllerBase
    {

        private readonly ILogger<SystemController> logger;
        private readonly IGlobalState globalData;

        /// <summary>
        /// CTor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="globalData"></param>
        public SystemController(ILogger<SystemController> logger, IGlobalState globalData)
        {
            this.logger = logger;
            this.globalData = globalData;
        }

        /// <summary>
        /// Changes the state of the requested relay
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Post([FromBody] SystemRequestMessageModel request)
        {
            //VALIDATE Request
            if (Request.HttpContext.User?.Claims != null && Request.HttpContext.User.Claims.Any())
            {
                string form = Request.HttpContext.User.Identity.Name;
                if (form != null && form.ToUpper() != "ADMIN")
                {
                    return new JsonResult(new { success = false, message = "this request cannot be validated as secure.  Please do not attempt to hack." });
                }
            }
            else
            {
                return new JsonResult(new { success = false, message = "this request cannot be validated as secure." });
            }

            logger.LogInformation("Changing the state of system " + request.State);
            if (string.IsNullOrEmpty(request.State) ||
                (request.State.ToLower() != "on" && request.State.ToLower() != "off"))
            {
                return new JsonResult(new { success = false, message = "state is not a valid value" });
            }

            try
            {
                globalData.SystemOnline = (request.State.ToUpper() == "ON");
            }
            catch (Exception ex)
            {
                logger.LogError("Failed to set state: " + ex.Message);
                return new JsonResult(new { success = false, message = "Failed to set state: " + ex.Message });
            }
            return new JsonResult(new { success = true, message = "State changed!" });
        }
    }
}