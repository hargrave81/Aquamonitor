using System;
using System.Linq;
using AquaMonitor.Data.Models;
using AquaMonitor.Web.Global;
using AquaMonitor.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PowerState = AquaMonitor.Data.Models.PowerState;

namespace AquaMonitor.Web.Controllers
{
    /// <summary>
    /// Relay Controller
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class RelayController : ControllerBase
    {

        private readonly ILogger<RelayController> logger;
        private readonly IPowerRelayService relayService;
        
        /// <summary>
        /// CTor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="relayService"></param>
        public RelayController(ILogger<RelayController> logger, IPowerRelayService relayService)
        {
            this.logger = logger;
            this.relayService = relayService;
        }

        /// <summary>
        /// Changes the state of the requested relay
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Post([FromBody] RelayRequestMessageModel request)
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


            logger.LogInformation("Changing the state of relay " + request.Relay);
            if (string.IsNullOrEmpty(request.Relay) ||
                (request.Relay.ToLower() != "a" && request.Relay.ToLower() != "b" && request.Relay.ToLower() != "c" && request.Relay.ToLower() != "d"))
            {
                return new JsonResult(new {success = false, message = "relay is not a valid value"});
            }
            if (string.IsNullOrEmpty(request.State) ||
                (request.State.ToLower() != "on" && request.State.ToLower() != "off"))
            {
                return new JsonResult(new { success = false, message = "state is not a valid value" });
            }

            try
            {
                relayService.SetState(request.Relay.ToLower() == "a" ? RelayLocation.A : request.Relay.ToLower() == "b" ? RelayLocation.B : request.Relay.ToLower() == "c" ? RelayLocation.C : RelayLocation.D, request.State == "on" ?  PowerState.On : PowerState.Off);
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