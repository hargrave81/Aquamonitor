using System.Diagnostics;
using AquaMonitor.Data.Context;
using AquaMonitor.Data.Models;
using Microsoft.AspNetCore.Mvc;
using AquaMonitor.Web.Models;

namespace AquaMonitor.Web.Controllers
{
    /// <summary>
    /// Home controller
    /// </summary>
    public class HomeController : Controller
    {
        private readonly IGlobalState globalData;
        private readonly AquaDbContext dbContext;

        /// <summary>
        /// CTor
        /// </summary>
        /// <param name="globalData"></param>
        /// <param name="dbContext"></param>
        public HomeController(IGlobalState globalData, AquaDbContext dbContext)
        {
            this.globalData = globalData;
            this.dbContext = dbContext;
        }

        /// <summary>
        /// Main Page
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            return View(globalData);
        }

        /// <summary>
        /// Partial table refresh
        /// </summary>
        /// <returns></returns>
        public IActionResult Tables()
        {
            return PartialView("_tables", globalData);
        }

        /// <summary>
        /// Partial header refresh
        /// </summary>
        /// <returns></returns>
        public IActionResult Headers()
        {
            return PartialView("_header");
        }

        /// <summary>
        /// Settings Page
        /// </summary>
        /// <returns></returns>
        public IActionResult Settings()
        {
            return View(globalData);
        }

        /// <summary>
        /// Settings Page
        /// </summary>
        /// <returns></returns>
        public IActionResult Readings()
        {
            return View(dbContext.GetLatestReadings());
        }

        /// <summary>
        /// Settings Page
        /// </summary>
        /// <returns></returns>
        public IActionResult Charts()
        {
            return View(globalData);
        }

        /// <summary>
        /// Error view
        /// </summary>
        /// <returns></returns>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
