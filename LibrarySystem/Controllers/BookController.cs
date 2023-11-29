using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibrarySystem.Context;
using LibrarySystem.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace LibrarySystem.Controllers;

public class BookController : Controller
{
    private readonly AppDbContext _context;
    
    // Dependency Injecting Database connection
    public BookController(AppDbContext context)
    {
        _context = context;
    }

    // GET: All books
    public async Task<IActionResult> ViewBooks()
    {
        var _Book = await _context.Books.ToListAsync();
        return View(_Book);
    }

    // Gets a list of books users has borrowed
    public async Task<IActionResult> UserBooks()
    {
        var user = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // var userBooks = _context.Borrowed.Where(u => u.MemberID == user).ToList();

        // More information on Left Join using LINQ found at https://stackoverflow.com/questions/3404975/left-outer-join-in-linq
        var userBooks = from Book in _context.Books 
                        join Borrowed in _context.Borrowed
                        on Book.BookID equals Borrowed.BookID into BorrowedBooks
                        from Borrowed in BorrowedBooks.DefaultIfEmpty()
                        where Borrowed.MemberID == user
                        select new UserBooks
                        {
                            BookTitle = Book.BookTitle,
                            Genre = Book.Genre,
                            Author = Book.Author,
                            BorrowedDate = Borrowed.BorrowedDate,
                            DueDate = Borrowed.DueDate,
                            ReturnedDate = Borrowed.ReturnedDate
                        };

        return View(userBooks);
    }

    public IActionResult AddBook()
    {
        return View();
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
    return RedirectToAction("Index");
    }

    // POST: Add Book
    [HttpPost]
    public async Task<IActionResult> NewBook([Bind("BookTitle,Genre, Author, Borrowed")] Book book)
    {
        _context.Books.Add(book);
        _context.SaveChanges();
        ViewBag.Message = "Data Insert Successfully";  
        return RedirectToAction("ViewBooks");
    }
}