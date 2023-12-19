using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using LibrarySystem.Models;
using Microsoft.AspNetCore.Identity;
using LibrarySystem.Context;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

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

    // Code tutorial used : https://www.freecodespot.com/blog/asp-net-core-identity/
public IActionResult Register()
    {
        return View();
    }

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
                await _userManager.AddToRoleAsync(user, "Member");
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

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login()
    {
        return View();
    }
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginViewModel user)
    {
        if (ModelState.IsValid)
        {
            var result = await _signInManager.PasswordSignInAsync(user.Email, user.Password, user.RememberMe, false);

            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Invalid Login Attempt");

        }
        return View(user);
    } 

    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();

        return RedirectToAction("Index","Home");
    }

    public async Task<IActionResult> ViewProfile()
    {

        var user = await _userManager.GetUserAsync(User);

        return View(user);
    }
}