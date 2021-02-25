using System;
using System.Linq;
using System.Threading.Tasks;
using AquaMonitor.Data.Context;
using AquaMonitor.Data.Models;
using AquaMonitor.Web.Helpers;
using AquaMonitor.Web.Models;
using Iot.Device.BrickPi3.Sensors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AquaMonitor.Web.Controllers
{
    /// <summary>
    /// Reading Controller
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class ReadingController : ControllerBase
    {
        private readonly AquaDbContext dbContext;
        private readonly ILogger<ReadingController> logger;


        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="dbContext"></param>
        public ReadingController(ILogger<ReadingController> logger, AquaDbContext dbContext)
        {
            this.logger = logger;
            this.dbContext = dbContext;
        }

        /// <summary>
        /// Gets the ChartJS for the given date range
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="type">Reading Type</param>
        /// <returns></returns>
        [HttpGet("")]
        public async Task<IActionResult> Get(string type, DateTime startDate, DateTime endDate)
        {
            if (!Enum.TryParse<ReadingType>(type, out var resultType))
                return this.BadRequest("Invalid Type");

            logger.LogInformation("Reading Chart has been requested");
            if (DateTime.Parse("1/1/2000") < startDate && DateTime.Parse("1/1/2000") < endDate &&
                DateTime.Parse("1/1/2600") > startDate && DateTime.Parse("1/1/2600") > endDate)
            {
                // we have a valid request
                var chartResult = await dbContext.GetChartAsync<ReadingChartJSModel>(resultType, startDate, endDate);
                var readingScale = new Reading() { Type = resultType }.ToType().ScaleString;
                chartResult.DataSets[0].PointBorderColor = Reading.ReadingColor(resultType);
                chartResult.DataSets[0].PointBackgroundColor = Reading.ReadingColor(resultType);
                chartResult.DataSets[0].BorderColor = Reading.ReadingColor(resultType);
                var color = System.Drawing.ColorTranslator.FromHtml(Reading.ReadingColor(resultType));
                chartResult.DataSets[0].BackgroundColor = $"rgba({color.R},{color.G},{color.B},.25)";
                chartResult.DataSets[0].Label = readingScale; // show the scale on the chart
                return new JsonResult(chartResult);
            }
            else
            {
                return this.BadRequest("Invalid Date Range");
            }
        }

        /// <summary>
        /// Gets the ChartJS for the given date range
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(IReading),200)]
        public async Task<IActionResult> Get(int id)
        {
            var result = await dbContext.Readings.FirstOrDefaultAsync(t => t.Id == id);
            if (result == null)
                return this.NoContent();
            return this.Ok(result);
        }

        /// <summary>
        /// Create new reading
        /// </summary>
        /// <param name="newReading"></param>
        /// <returns></returns>
        [ValidateAntiForgeryToken]
        [ProducesResponseType(typeof(ProblemDetails), 500)]
        [ProducesResponseType(typeof(ModelStateDictionary), 400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(typeof(IReading),201)]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ReadingRequestModel newReading)
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

            if (!ModelState.IsValid)
            {
                return this.BadRequest(ModelState);
            }

            try
            {
                var finalReading = newReading.ToReading(out var errors);
                if (!string.IsNullOrEmpty(errors))
                {
                    return this.BadRequest(finalReading); // we had errors processing the parsing
                }
                var result = await dbContext.SaveReadingAsync(finalReading);
                if (result != null)
                    return this.Created(new Uri($"{Request.Path}/{result.Id}", UriKind.Relative), result);
                return this.Problem("Could not create the requested reading.");
            }
            catch (Exception ex)
            {
                return this.Problem("Could not create the requested reading due to "+ex.Message);
            }
        }
    }
}