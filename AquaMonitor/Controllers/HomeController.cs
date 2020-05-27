using System.Diagnostics;
using System.Threading.Tasks;
using AquaMonitor.Data.Models;
using Microsoft.AspNetCore.Mvc;
using AquaMonitor.Web.Global;
using AquaMonitor.Web.Models;
using Microsoft.AspNetCore.Identity;

namespace AquaMonitor.Web.Controllers
{
    /// <summary>
    /// Home controller
    /// </summary>
    public class HomeController : Controller
    {
        private readonly IGlobalState globalData;
        private readonly IPowerRelayService relayService;

        /// <summary>
        /// CTor
        /// </summary>
        /// <param name="globalData"></param>
        /// <param name="relayService"></param>
        public HomeController(IGlobalState globalData, IPowerRelayService relayService)
        {
            this.globalData = globalData;
            this.relayService = relayService;
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
