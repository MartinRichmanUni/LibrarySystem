using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace LibrarySystem.Models;

public class RegisterViewModel
{
    [PersonalData]
    public string FirstName { get; set; }
    [PersonalData]
    public string LastName { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }
    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    [DataType(DataType.Password)]
    [Display(Name ="Confirm Password")]
    [Compare("Password",ErrorMessage ="Password and confirmation password don't match.")]
    public string ConfirmPassword { get; set; }
}