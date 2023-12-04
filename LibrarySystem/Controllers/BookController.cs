using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibrarySystem.Context;
using LibrarySystem.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LibrarySystem.Controllers;

public class BookController : Controller
{
    private readonly AppDbContext _context;
    
    // Dependency Injecting Database connection
    public BookController(AppDbContext context)
    {
        _context = context;
    }

    // Either returns all books or based on search results
    // Using https://learn.microsoft.com/en-us/aspnet/core/tutorials/first-mvc-app/search?view=aspnetcore-8.0 as a tutorial
    public async Task<IActionResult> ViewBooks(string bookGenre, string searchResult)
    {
        if (_context.Books == null)
        {
            return Problem("No Books are within the system");
        }

        // List of Genres
        IQueryable<string> genreQuery = from b in _context.Books
                                    orderby b.Genre
                                    select b.Genre;

        var books = from b in _context.Books
                    select b;

        if (!String.IsNullOrEmpty(searchResult))
        {
            books = books.Where(m => m.BookTitle!.Contains(searchResult));
        }

        if (!String.IsNullOrEmpty(bookGenre))
        {
            books = books.Where(g => g.Genre == bookGenre);
        }

        var bookGenreView = new BookGenreViewModel
        {
            Genres = new SelectList(await genreQuery.Distinct().ToListAsync()),
            Books = await books.ToListAsync()
        };

        return View(bookGenreView);
    }

    // Gets a list of books users has borrowed
    public async Task<IActionResult> UserBooks()
    {
        var user = User.FindFirstValue(ClaimTypes.NameIdentifier);

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

    // Look at ways to set DueDate to user's chosen date + 14 days
    // ReturnedDate is not bound but still returns the date as "01/01/0001" within the database
    [HttpPost]
    public IActionResult BorrowBook([Bind("BorrowedDate, DueDate, MemberID, BookID")] Borrowed uBook)
    {
        _context.Borrowed.Add(uBook);
        _context.SaveChanges();
        return RedirectToAction("ViewBooks");
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
        ViewBag.Message = "Book Added Successfully";  
        return RedirectToAction("ViewBooks");
    }
}