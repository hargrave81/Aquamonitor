using System;
using System.Threading.Tasks;
using AquaMonitor.Data.Context;
using AquaMonitor.Web.Helpers;
using AquaMonitor.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AquaMonitor.Web.Controllers
{
    /// <summary>
    /// Temperature Chart Controller
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class OutsideTempChartController : ControllerBase
    {
        private readonly AquaDbContext dbContext;
        private readonly ILogger<OutsideTempChartController> logger;


        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="dbContext"></param>
        public OutsideTempChartController(ILogger<OutsideTempChartController> logger, AquaDbContext dbContext)
        {
            this.logger = logger;
            this.dbContext = dbContext;
        }

        /// <summary>
        /// Gets the ChartJS for the given date range
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Get(DateTime startDate, DateTime endDate)
        {
            logger.LogInformation("Temp Chart has been requested");
            if (DateTime.Parse("1/1/2000") < startDate && DateTime.Parse("1/1/2000") < endDate &&
                DateTime.Parse("1/1/2600") > startDate && DateTime.Parse("1/1/2600") > endDate)
            {
                // we have a valid request
                var chartResult = await dbContext.GetChartAsync<OutsideTempChartModel>(startDate, endDate);

                return new JsonResult(chartResult);
                
            }
            else
            {
                return this.BadRequest();
            }
        }
    }
}