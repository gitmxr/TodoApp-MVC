using Azure.Core;
using Azure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Security.Claims;
using TodoApp.Models;
using TodoApp.ViewModels;

namespace TodoApp.Controllers
{
    public class AccountController : Controller
    {

        public readonly UserManager<User> _userManager;
        public readonly SignInManager<User> _signInManager;

        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }
        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // Post action /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new User
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.FullName
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // ✅ Instead of auto sign-in, send user to Login
                    return RedirectToAction("Login", "Account");
                }

                // If we got here → errors happened during CreateAsync
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            // If ModelState is invalid or creation failed → redisplay form
            return View(model);
        }

        // Get: login

        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            // Step 1: Validate the form inputs
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Step 2: Try to log the user in using SignInManager
            var result = await _signInManager.PasswordSignInAsync(
                model.Email,                // the username/email
                model.Password,             // the password
                false,                      // rememberMe (you can make this part of your model later)
                lockoutOnFailure: false     // lock account after failed attempts (not needed now)
            );

            // Step 3: Check if login succeeded
            if (result.Succeeded)
            {
                // 1. Get the user
                var user = await _userManager.FindByEmailAsync(model.Email);

                // 2. Create extra claims (FullName)
                var claims = new List<Claim>
                {
                  new Claim("FullName", user.FullName ?? user.Email)
                };

                // 3. Sign in with claims
                await _signInManager.SignInWithClaimsAsync(user, isPersistent: model.RememberMe, claims);

                // 4. Redirect to Todos
                return RedirectToAction("Index", "Todos");
            }

            // Step 4: If login failed → show error on the same page
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }
          
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account"); // send back to login page
        }


    }
}
