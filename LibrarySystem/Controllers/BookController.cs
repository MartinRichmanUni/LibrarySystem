using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibrarySystem.Context;
using LibrarySystem.Models;

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
        return RedirectToAction("Index");
    }
}