using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AquaMonitor.Data.Context;
using AquaMonitor.Data.Models;
using AquaMonitor.Web.Global;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AquaMonitor.Web.Controllers
{
    /// <summary>
    /// Account level controller
    /// </summary>
    public class AccountController : Controller
    {
        private readonly IGlobalState globalData;
        private readonly UserManager<AppUser> userManager;
        private readonly SignInManager<AppUser> signInManager;
        private readonly AquaDbContext dbContext;

        /// <summary>
        /// CTor
        /// </summary>
        /// <param name="globalData"></param>
        /// <param name="dbContext"></param>
        /// <param name="userManager"></param>
        /// <param name="signInManager"></param>
        public AccountController(IGlobalState globalData, AquaDbContext dbContext,
            UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            this.globalData = globalData;
            this.dbContext = dbContext;
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        #region "Account Section"

        /// <summary>
        /// Action used to login into system
        /// </summary>
        /// <param name="appUser"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([FromBody] AppUser appUser)
        {

            //login functionality  
            var user = await userManager.FindByNameAsync(appUser.UserName);

            if (user != null)
            {
                //sign in  
                var signInResult = await signInManager.PasswordSignInAsync(user, appUser.Password, false, false);

                if (signInResult.Succeeded)
                {
                    return new JsonResult(new {success = true, message = "Logged in"});
                }
            }

            return (new JsonResult(new {success = false, message = "Failed to authenticate"}) {StatusCode = 401});
        }

        /// <summary>
        /// Used to logout
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> LogOut()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Index","Home");
        }
        #endregion

        /// <summary>
        /// Gets the user profile
        /// </summary>
        /// <returns></returns>
        [HttpGet()]
        public async Task<IActionResult> Profile()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{userManager.GetUserId(User)}'.");
            }

            return View(new PasswordModel
            {
                
            });
        }

        /// <summary>
        /// Updates the user profile
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [ValidateAntiForgeryToken]
        [HttpPost()]
        public async Task<IActionResult> UpdateProfile(
            [FromForm]
            PasswordModel input)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Profile));
            }

            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{userManager.GetUserId(User)}'.");
            }

            if(input.Password != input.PasswordConfirm)
            {
                return BadRequest($"Passwords do not match.");
            }

            var setting = dbContext.GetSetting();
            setting.AdminPassword = input.Password;
            await dbContext.SaveSettingsAsync(setting);
            
            return RedirectToAction(nameof(Profile));
        }
    }
}