using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using LibrarySystem.Models;
using Microsoft.AspNetCore.Identity;
using LibrarySystem.Context;
using Microsoft.AspNetCore.Authorization;

using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace LibrarySystem.Controllers;

public class AccountController : Controller
{
private readonly UserManager<ApplicationUser> _userManager;
private readonly SignInManager<ApplicationUser> _signInManager;

private readonly IEmailSender _emailSender;

public AccountController(UserManager<ApplicationUser> userManager,
                              SignInManager<ApplicationUser> signInManager,
                              IEmailSender emailSender)
    {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
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

    public async Task<IActionResult> ChangeEmail()
    {
        var user = await _userManager.GetUserAsync(User);

        return View(user);
    }

    /** Look at correct validation methods for ensuring email is valid **/
    [HttpPost]
    public async Task<IActionResult> ChangeEmail(string email)
    {
        var user = await _userManager.GetUserAsync(User);
            user.Email = email;
            user.UserName = email;

            await _userManager.UpdateAsync(user);

            return RedirectToAction("ViewProfile");
    }

    public IActionResult CheckEmail()
    {
        return View();
    }

    /**
        NOTE: This function includes temporary data for testing. 
        A better option would be to use a template provided by SendGrid, or to create one.
        The link is a localhost and within a real world environment, the real web address should be used.
    **/
    [HttpPost]
    public async Task<IActionResult> CheckEmail(string email)
    {
        var user = await _userManager.GetUserAsync(User);
        string subject = "Password Reset for Library Management System";
        string body = "This is a password reset, please click <a href='http://localhost:5027/Account/ChangePassword'>here<a> to open the link to reset the password associated with your account.";

        if (user.Email == email)
        {
            await _emailSender.SendEmailAsync(user.Email, subject, body);
        }

        return RedirectToAction("ViewProfile");
    }

    public IActionResult ChangePassword()
    {
        return View();
    }

    /** Look at validation for password to ensure it matches the minimum requirements set out **/
    /**
        Toke provider for password does not work, need to figure out a solution
    [HttpPost]
    public async Task<IActionResult> ChangePassword(string password)
    {
        var user = await _userManager.GetUserAsync(User);
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, password);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, "Invalid Login Attempt");
            return View();
        }

        return RedirectToAction("Index", "Home");
    }
    **/
}