using Microsoft.AspNetCore.Mvc;
using LibrarySystem.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using LibrarySystem.Models;
using Microsoft.AspNetCore.Authorization;

namespace LibrarySystem.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    // private readonly UserManager<ApplicationUser> _userManager;
    private readonly AppDbContext _context;

    public AdminController(AppDbContext context)
    {
        // _userManager = userManager;
        _context = context;
    }
    
    public IActionResult ViewAllUsers()
    {
        var users = from m in _context.Users
                    select new UserModel
                    {
                        Id = m.Id,
                        FirstName = m.FirstName,
                        LastName = m.LastName,
                        Email = m.Email,
                    };
        return View(users);
    }

    public IActionResult ViewUser(string id)
    {
        var userBooks = from Book in _context.Books 
                        join Borrowed in _context.Borrowed
                        on Book.BookID equals Borrowed.BookID into BorrowedBooks
                        from Borrowed in BorrowedBooks.DefaultIfEmpty()
                        where Borrowed.MemberID == id
                        select new UserBooks
                        {
                            BookID = Book.BookID,
                            BookTitle = Book.BookTitle,
                            Genre = Book.Genre,
                            Author = Book.Author,
                            BorrowedDate = Borrowed.BorrowedDate,
                            DueDate = Borrowed.DueDate,
                            ReturnedDate = Borrowed.ReturnedDate
                        };

        return View(userBooks);
    }
}