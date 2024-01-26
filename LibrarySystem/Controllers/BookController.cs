using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibrarySystem.Context;
using LibrarySystem.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Rendering;
using X.PagedList;

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
    // https://learn.microsoft.com/en-us/aspnet/mvc/overview/getting-started/getting-started-with-ef-using-mvc/sorting-filtering-and-paging-with-the-entity-framework-in-an-asp-net-mvc-application
    public async Task<IActionResult> ViewBooks(string bookGenre, string searchResult, string sortOrder, string currentFilter, int? page)
    {
        if (_context.Books == null)
        {
            return Problem("No Books are within the system");
        }

        IQueryable<string> genreQuery = from b in _context.Books
                                    orderby b.Genre
                                    select b.Genre;
        
        ViewBag.CurrentSort = sortOrder;
        ViewBag.TitleSort = String.IsNullOrEmpty(sortOrder) ? "title_desc" : "";
        ViewBag.GenreSort = sortOrder == "Genre" ? "genre_desc" : "Genre";
        ViewBag.AuthorSort = sortOrder == "Author" ? "author_desc" : "Author";

        if (searchResult != null)
        {
            page = 1;
        }
        else 
        {
            searchResult = currentFilter;
        }

        ViewBag.currentFilter = searchResult;

        var books = from b in _context.Books
                    select b;

        switch (sortOrder)
        {
            case "title_desc":
                books = books.OrderByDescending(b => b.BookTitle);
                break;
            case "genre_desc":
                books = books.OrderByDescending(b => b.Genre);
                break;
            case "Genre":
                books = books.OrderBy(b => b.Genre);
                break;
            case "author_desc":
                books = books.OrderByDescending(b => b.Author);
                break;
            case "Author":
                books = books.OrderBy(b => b.Author);
                break;
            default:
                books = books.OrderBy(b => b.BookTitle);
                break;
        }

        if (!String.IsNullOrEmpty(searchResult))
        {
            books = books.Where(m => m.BookTitle!.Contains(searchResult));
        }

        if (!String.IsNullOrEmpty(bookGenre))
        {
            books = books.Where(g => g.Genre == bookGenre);
        }

        int pageSize = 10;
        int pageNumber = (page ?? 1);
        var bookGenreView = new BookGenreViewModel
        {
            Genres = new SelectList(await genreQuery.Distinct().ToListAsync()),
            Books = books.ToPagedList(pageNumber, pageSize)
        };

        return View(bookGenreView);
    }

    // Gets a list of books users has borrowed
    public IActionResult ViewUserBooks(string searchResult, string sortOrder, string currentFilter, int? page, string status)
    {
        var userID = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return ViewComponent("UserBooks",
                    new {
                        id = userID,
                        searchResult,
                        sortOrder,
                        currentFilter,
                        page,
                        status
                    });
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

    // ReturnedDate is not bound but returns "null" within the database
    [HttpPost]
    public async Task<IActionResult> BorrowBook([Bind("BorrowedDate, DueDate, MemberID, BookID")] Borrowed uBook)
    {
        uBook.DueDate = uBook.BorrowedDate.AddDays(14);
        _context.Borrowed.Add(uBook);
        await _context.SaveChangesAsync();

        var stockChange = await _context.Books.FirstOrDefaultAsync(b => b.BookID == uBook.BookID) ?? throw new Exception("ID not found");
        stockChange.StockAmount -= 1;
        await _context.SaveChangesAsync();
        
        return RedirectToAction("ViewBooks");
    }
    
   

}