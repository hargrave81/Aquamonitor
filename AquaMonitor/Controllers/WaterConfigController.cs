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
    /// WaterLevel Controller
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class WaterConfigController : ControllerBase
    {

        private readonly ILogger<WaterConfigController> logger;        
        private readonly AquaDbContext dbContext;
        private readonly IGlobalState globalData;

        /// <summary>
        /// CTor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="dbContext"></param>
        /// <param name="globalData"></param>
        public WaterConfigController(ILogger<WaterConfigController> logger, AquaDbContext dbContext,
                                     IGlobalState globalData)
        {
            this.logger = logger;            
            this.dbContext = dbContext;
            this.globalData = globalData;
        }

        /// <summary>
        /// Updates the waterlevels in the system
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Post([FromBody] WaterLevelRequestMessageModel request)
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


            logger.LogInformation("Changing the water levels");
            
            try
            {
                var allWaters = await dbContext.GetWaterLevelsAsync();
                var deletables = allWaters.Where(t => !request.WaterLevels.Select(q => q.Id).Contains(t.Id));
                var adds = new List<WaterLevel>();
                foreach(var relay in request.WaterLevels)
                {
                    if(allWaters.Any(t => t.Id == relay.Id))
                    {
                        // edit
                        relay.UpdateWaterLevel(allWaters.First(t => t.Id == relay.Id));
                    }
                    else
                    {
                        // add it
                        adds.Add(relay.ToWaterLevel());
                    }
                }
                // perform DB operations
                await dbContext.DeleteWaterLevelsAsync(deletables);
                await dbContext.AddWaterLevelsAsync(adds);
                await dbContext.UpdateWaterLevelsAsync(allWaters.Where(t => !deletables.Contains(t)));       
                globalData.WaterLevels = allWaters.Where(t => !deletables.Contains(t)).ToList();         
            }
            catch (Exception ex)
            {
                logger.LogError("Failed to set waterlevels: " + ex.Message);
                return new JsonResult(new { success = false, message = "Failed to set waterlevels: " + ex.Message });
            }
            return new JsonResult(new { success = true, message = "WaterLevels changed!" });
        }
    }
}