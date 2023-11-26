using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using LibrarySystem.Models;
using Microsoft.AspNetCore.Identity;
using LibrarySystem.Context;

namespace LibrarySystem.Controllers;

public class AccountController : Controller
{
private readonly UserManager<ApplicationUser> _userManager;
private readonly SignInManager<ApplicationUser> _signInManager;

public AccountController(UserManager<ApplicationUser> userManager,
                              SignInManager<ApplicationUser> signInManager)
    {
            _userManager = userManager;
            _signInManager = signInManager;
    }

public IActionResult Register()
    {
        return View();
    }

    // User Account Registration
    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = new ApplicationUser
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                UserName = model.Email,
                Email = model.Email,
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);

                return RedirectToAction("index", "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            ModelState.AddModelError(string.Empty, "Please Enter All Information Required");

        }
        return View(model);
    }
}