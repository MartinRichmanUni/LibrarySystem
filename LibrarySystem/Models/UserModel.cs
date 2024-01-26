using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace LibrarySystem.Models;

public class UserModel
{
    [PersonalData]
    public string Id { get; set;}

    [PersonalData]
    public string FirstName { get; set; }

    [PersonalData]
    public string LastName { get; set; }

    [PersonalData]
    [EmailAddress]
    public string Email { get; set; }

    [DataType(DataType.Password)]
    [PersonalData]
    public string Password { get; set; }

}