using System;
using System.Linq;
using AquaMonitor.Data.Context;
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
                if (form != null && form.ToUpper() != "ADMIN")
                {
                    return new JsonResult(new { success = false, message = "this request cannot be validated as secure.  Please do not attempt to hack." });
                }
            }
            else
            {
                return new JsonResult(new { success = false, message = "this request cannot be validated as secure." });
            }

            if (request.TempType != "11" && request.TempType != "22" && request.TempType != "21" && request.TempType != "28")
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
                globalData.More.CameraJPGUrl = request.More_CameraJPGUrl;
                globalData.More.WaterSensorEnabled = request.More_WaterSensorEnabled;
                // handle more feed
                globalData.More.FoodSessions = new FoodSession[3];
                globalData.More.FoodSessions[0] = new FoodSession()
                {
                    PinCollection = request.More_FeedingPins,
                    StartTime = !string.IsNullOrEmpty(request.More_Feed1_Start)
                        ? (TimeSpan?) TimeSpan.Parse(request.More_Feed1_Start)
                        : null,
                    TurnTime = !string.IsNullOrEmpty(request.More_Feed1_Turn) ? int.Parse(request.More_Feed1_Turn.Split(',')[0]):0
                };
                globalData.More.FoodSessions[1] = new FoodSession()
                {
                    PinCollection = request.More_FeedingPins,
                    StartTime = !string.IsNullOrEmpty(request.More_Feed2_Start)
                        ? (TimeSpan?) TimeSpan.Parse(request.More_Feed2_Start)
                        : null,
                    TurnTime = !string.IsNullOrEmpty(request.More_Feed2_Turn) ? int.Parse(request.More_Feed2_Turn.Split(',')[0]):0
                };
                globalData.More.FoodSessions[2] = new FoodSession()
                {
                    PinCollection = request.More_FeedingPins,
                    StartTime = !string.IsNullOrEmpty(request.More_Feed3_Start)
                        ? (TimeSpan?) TimeSpan.Parse(request.More_Feed3_Start)
                        : null,
                    TurnTime = !string.IsNullOrEmpty(request.More_Feed3_Turn) ? int.Parse(request.More_Feed3_Turn.Split(',')[0]):0
                };
                // swap out optional different pin for different feeder
                if(!string.IsNullOrEmpty(request.More_Feed1_Turn))
                {
                    if (request.More_Feed1_Turn.Split(',').Length > 1)
                    {
                        var pc = globalData.More.FoodSessions[0].PinCollection.Split(',');
                        pc[0] = request.More_Feed1_Turn.Split(',')[1];
                        globalData.More.FoodSessions[0].PinCollection = string.Join(',', pc);
                    }
                }
                if(!string.IsNullOrEmpty(request.More_Feed2_Turn))
                {
                    if (request.More_Feed2_Turn.Split(',').Length > 1)
                    {
                        var pc = globalData.More.FoodSessions[1].PinCollection.Split(',');
                        pc[0] = request.More_Feed2_Turn.Split(',')[1];
                        globalData.More.FoodSessions[1].PinCollection = string.Join(',', pc);
                    }
                }
                if(!string.IsNullOrEmpty(request.More_Feed3_Turn))
                {
                    if (request.More_Feed3_Turn.Split(',').Length > 1)
                    {
                        var pc = globalData.More.FoodSessions[2].PinCollection.Split(',');
                        pc[0] = request.More_Feed3_Turn.Split(',')[1];
                        globalData.More.FoodSessions[2].PinCollection = string.Join(',', pc);
                    }
                }
                // now save to database 
                var settings = dbContext.GetSetting();
                settings.TempType = globalData.TempType;
                settings.TempPin = globalData.TempPin;
                settings.DataCollectionRate = globalData.DataCollectionRate;
                settings.Country = globalData.Country;
                settings.Zipcode = globalData.Zipcode;
                settings.APIKey = globalData.APIKey;
                settings.More.TempOffset = globalData.More.TempOffset;
                settings.More.CameraJPGUrl = globalData.More.CameraJPGUrl;
                settings.More.FoodSessions = globalData.More.FoodSessions;
                settings.More.WaterSensorEnabled = globalData.More.WaterSensorEnabled;
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