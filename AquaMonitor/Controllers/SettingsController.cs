using System;
using System.Linq;
using AquaMonitor.Data.Context;
using AquaMonitor.Data.Models;
using AquaMonitor.Web.Global;
using AquaMonitor.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AquaMonitor.Web.Controllers
{
    /// <summary>
    /// Relay Controller
    /// </summary>
    [Route("[controller]")]
    [ApiController]    
    public class SettingsController : ControllerBase
    {

        private readonly ILogger<SettingsController> logger;
        private readonly IGlobalState globalData;
        private readonly AquaDbContext dbContext;

        /// <summary>
        /// CTor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="globalData"></param>
        /// <param name="dbContext"></param>
        public SettingsController(ILogger<SettingsController> logger, IGlobalState globalData, AquaDbContext dbContext)
        {
            this.logger = logger;
            this.globalData = globalData;
            this.dbContext = dbContext;
        }

        /// <summary>
        /// Changes the state of the requested relay
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Post([FromBody] SettingsRequestMessageModel request)
        {
            logger.LogInformation("Changing the settings of the system ");
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

            if (request.TempType != "11" && request.TempType != "22" && request.TempType != "21")
            {
                return new JsonResult(new { success = false, message = "Temp type is invalid" });
            }
            if (request.TempPin != "4" && request.TempPin != "7" && request.TempPin != "26" && request.TempPin != "1")
            {
                return new JsonResult(new { success = false, message = "Temp pin must be 1, 4, 7 or 26" });
            }

            if (request.DataCollectionRate < 10 || request.DataCollectionRate > 3600)
            {
                return new JsonResult(new { success = false, message = "Data collection rate must be between 10 seconds and 3600 seconds (1hr)" });
            }
            try
            {
                globalData.Zipcode = request.Zipcode;
                globalData.Country = request.Country;
                globalData.APIKey = request.APIKey;
                globalData.TempPin = int.Parse(request.TempPin);
                globalData.TempType = int.Parse(request.TempType);
                globalData.DataCollectionRate = request.DataCollectionRate;
                globalData.More.TempOffset = request.More_TempOffset;
                // now save to database    
                var settings = dbContext.GetSetting();
                settings.TempType = globalData.TempType;
                settings.TempPin = globalData.TempPin;
                settings.DataCollectionRate = globalData.DataCollectionRate;
                settings.Country = globalData.Country;
                settings.Zipcode = globalData.Zipcode;
                settings.APIKey = globalData.APIKey;
                settings.More.TempOffset = globalData.More.TempOffset;
                dbContext.SaveSettings(settings);
            }
            catch (Exception ex)
            {
                logger.LogError("Failed to save changes: " + ex.Message);
                return new JsonResult(new { success = false, message = "Failed to save changes: " + ex.Message });
            }
            return new JsonResult(new { success = true, message = "Changes saved!" });
        }
    }
}