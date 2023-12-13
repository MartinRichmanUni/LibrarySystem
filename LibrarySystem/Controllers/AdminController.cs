using Microsoft.AspNetCore.Mvc;
using LibrarySystem.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using LibrarySystem.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace LibrarySystem.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly AppDbContext _context;

    public AdminController(AppDbContext context)
    {
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
        ViewData["id"] = id;
        return View(userBooks);
    }

    public IActionResult EditBook(int id)
    {
        var book = (from b in _context.Books
                    where b.BookID == id
                    select new Book
                    {
                        BookID = b.BookID,
                        BookTitle = b.BookTitle,
                        Genre = b.Genre,
                        Author = b.Author,
                        StockAmount = b.StockAmount
                    }).FirstOrDefault();

        return View(book);
    }

    [HttpPost]
    public async Task<IActionResult> EditBook(int id, [Bind("BookTitle, Author, Genre, StockAmount")] Book book)
    {
        var bookUpdate = _context.Books.Find(id);

        if (bookUpdate == null)
        {
            return NotFound();
        }

        bookUpdate.BookTitle = book.BookTitle;
        bookUpdate.Author = book.Author;
        bookUpdate.Genre = book.Genre;
        bookUpdate.StockAmount = book.StockAmount;

        await _context.SaveChangesAsync();

        return RedirectToAction("ViewBooks");
    }

    public IActionResult AddBook()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> AddBook([Bind("BookTitle,Genre, Author, StockAmount")] Book book)
    {
        _context.Books.Add(book);
        await _context.SaveChangesAsync();
        ViewBag.Message = "Book Added Successfully";  
        return RedirectToAction("ViewBooks", "Book");
    }

    public IActionResult BorrowBook(int id)
    {
        var user = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var book = (from b in _context.Books
                    where b.BookID == id
                    select new UserBooks
                    {
                        BookID = b.BookID,
                        BookTitle = b.BookTitle,
                        Genre = b.Genre,
                        Author = b.Author,
                        MemberID = user
                    }
                    
                    ).FirstOrDefault();

        return View(book);
    }

     public async Task<IActionResult> DeleteBook(int? id)
    {
    if (id == null)
    {
        return NotFound();
    }

    var book = await _context.Books.FirstOrDefaultAsync(b => b.BookID == id);
    if (book == null)
    {
        return NotFound();
    }
    _context.Books.Remove(book);
    await _context.SaveChangesAsync();
    return RedirectToAction("ViewBooks", "Book");
    }
}