using Microsoft.AspNetCore.Mvc;
using LibrarySystem.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using LibrarySystem.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using X.PagedList;

namespace LibrarySystem.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly AppDbContext _context;

    public AdminController(AppDbContext context)
    {
        _context = context;
    }
    
    public IActionResult ViewAllUsers(string sortOrder, string currentFilter, string searchResult, int? page)
    {

        ViewBag.CurrentSort = sortOrder;
        ViewBag.FNameSort = String.IsNullOrEmpty(sortOrder) ? "FNameDesc" : "";
        ViewBag.LNameSort = sortOrder == "LastName" ? "LNameDesc" : "LastName";
        ViewBag.Email = sortOrder == "Email" ? "EmailDesc" : "Email";

        if (searchResult != null)
        {
            page = 1;
        } 
        else
        {
            searchResult = currentFilter;
        }

        ViewBag.currentFilter = searchResult;

        var users = from m in _context.Users
                    select new UserModel
                    {
                        Id = m.Id,
                        FirstName = m.FirstName,
                        LastName = m.LastName,
                        Email = m.Email,
                    };

        switch(sortOrder)
        {
            case "FNameDesc":
                users = users.OrderByDescending(u => u.FirstName);
                break;
            case "LastName":
                users = users.OrderBy(u => u.LastName);
                break;
            case "LNameDesc":
                users = users.OrderByDescending(u => u.LastName);
                break;
            case "Email":
                users = users.OrderBy(u => u.Email);
                break;
            case "EmailDesc":
                users = users.OrderByDescending(u => u.Email);
                break;
            default:
                users = users.OrderBy(u => u.FirstName);
                break;  
        }

        int pageSize = 10;
        int pageNumber = (page ?? 1);
        if (!String.IsNullOrEmpty(searchResult))
        {
            users = users.Where(u => u.FirstName.Contains(searchResult));
        }

        return View(users.ToPagedList(pageNumber, pageSize));
    }

    public IActionResult ViewUserBooks(string id, string searchResult, string sortOrder, string currentFilter, int? page, string status)
    {

        return ViewComponent("UserBooks",
                    new {
                        id = id,
                        searchResult = searchResult,
                        sortOrder = sortOrder,
                        currentFilter = currentFilter,
                        page = page,
                        status = status
                    });
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

        return RedirectToAction("ViewBooks", "Book");
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

    public IActionResult ReturnBook(string userID, int bookID)
    {
        var userBook = (from Book in _context.Books 
                        join Borrowed in _context.Borrowed
                        on Book.BookID equals Borrowed.BookID into BorrowedBooks
                        from Borrowed in BorrowedBooks.DefaultIfEmpty()
                        where Borrowed.MemberID == userID && Borrowed.BookID == bookID
                        select new UserBooks
                        {
                            BookTitle = Book.BookTitle,
                            Genre = Book.Genre,
                            Author = Book.Author,
                            BorrowedDate = Borrowed.BorrowedDate,
                            DueDate = Borrowed.DueDate,
                            ReturnedDate = DateOnly.FromDateTime(DateTime.Now),
                            MemberID = Borrowed.MemberID
                        }).FirstOrDefault();
        return View(userBook);
    }

    [HttpPost]
    public async Task<IActionResult> ReturnBook(DateOnly returnedDate, int bookID, string memberID)
    {
        var borrowedBook = (from b in _context.Borrowed
                            where b.BookID == bookID && b.MemberID == memberID && b.ReturnedDate == null
                            select b ).FirstOrDefault();

        borrowedBook.ReturnedDate = returnedDate;

        var stockUpdate = (from b in _context.Books
                            where b.BookID == bookID
                            select b).FirstOrDefault();
        
        stockUpdate.StockAmount += 1;

        await _context.SaveChangesAsync();

        return RedirectToAction("ViewAllUsers");
    }
}