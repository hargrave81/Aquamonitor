using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AquaMonitor.Data.Context;
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
    public class RelayConfigController : ControllerBase
    {

        private readonly ILogger<RelayConfigController> logger;        
        private readonly AquaDbContext dbContext;

        /// <summary>
        /// CTor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="dbContext"></param>
        public RelayConfigController(ILogger<RelayConfigController> logger, AquaDbContext dbContext)
        {
            this.logger = logger;            
            this.dbContext = dbContext;
        }

        /// <summary>
        /// Updates the relays in the system
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Post([FromBody] PowerRelayRequestMessageModel request)
        {
            //VALIDATE Request
            if (Request.HttpContext.User?.Claims != null && Request.HttpContext.User.Claims.Any())
            {
                string form = Request.HttpContext.User.Identity.Name;
                if (form.ToUpper() != "ADMIN")
                {
                    return new JsonResult(new { success = false, message = "this request cannot be validated as secure.  Please do not attempt to hack." });
                }
            }
            else
            {
                return new JsonResult(new { success = false, message = "this request cannot be validated as secure." });
            }


            logger.LogInformation("Changing the relays");
            
            try
            {
                var allRelays = await dbContext.GetRelaysAsync();
                var deletables = allRelays.Where(t => !request.Relays.Select(q => q.Id).Contains(t.Id));
                var adds = new List<PowerRelay>();
                foreach(var relay in request.Relays)
                {
                    if(allRelays.Any(t => t.Id == relay.Id))
                    {
                        // edit
                        relay.UpdateRelay(allRelays.First(t => t.Id == relay.Id));
                    }
                    else
                    {
                        // add it
                        adds.Add(relay.ToPowerRelay());
                    }
                }
                // perform DB operations
                await dbContext.DeleteRelaysAsync(deletables);
                await dbContext.AddRelaysAsync(adds);
                await dbContext.UpdateRelaysAsync(allRelays.Where(t => !deletables.Contains(t)));                
            }
            catch (Exception ex)
            {
                logger.LogError("Failed to set relays: " + ex.Message);
                return new JsonResult(new { success = false, message = "Failed to set relays: " + ex.Message });
            }
            return new JsonResult(new { success = true, message = "Relays changed!" });
        }
    }
}