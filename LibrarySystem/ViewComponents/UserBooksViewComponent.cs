using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibrarySystem.Context;
using LibrarySystem.Models;
using X.PagedList;

namespace LibrarySystem.ViewComponents;

public class UserBooksViewComponent : ViewComponent
{
    private readonly AppDbContext _context;
    
    // Dependency Injecting Database connection
    public UserBooksViewComponent(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IViewComponentResult> InvokeAsync(string id, string searchResult, string sortOrder, string currentFilter, int? page, string status)
    {
        ViewBag.CurrentSort = sortOrder;
        ViewBag.TitleSort = String.IsNullOrEmpty(sortOrder) ? "title_desc" : "";
        ViewBag.GenreSort = sortOrder == "Genre" ? "genre_desc" : "Genre";
        ViewBag.AuthorSort = sortOrder == "Author" ? "email_desc" : "Author";

        if (searchResult != null)
        {
            page = 1;
        } 
        else
        {
            searchResult = currentFilter;
        }

        ViewBag.currentFilter = searchResult;


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
                            ReturnedDate = Borrowed.ReturnedDate,
                            MemberID = Borrowed.MemberID
                        };

        switch(sortOrder)
        {
            case "title_desc":
                userBooks = userBooks.OrderByDescending(u => u.BookTitle);
                break;
            case "Genre":
                userBooks = userBooks.OrderBy(u => u.Genre);
                break;
            case "genre_desc":
                userBooks = userBooks.OrderByDescending(u => u.Genre);
                break;
            case "Author":
                userBooks = userBooks.OrderBy(u => u.Author);
                break;
            case "author_desc":
                userBooks = userBooks.OrderByDescending(u => u.Author);
                break;
            default:
                userBooks = userBooks.OrderBy(u => u.BookTitle);
                break;  
        }
        
        int pageSize = 2;
        int pageNumber = (page ?? 1);
        if (!String.IsNullOrEmpty(searchResult))
        {
            userBooks = userBooks.Where(u => u.BookTitle!.Contains(searchResult));
        }

        if (status == "Returned")
        {
            userBooks = userBooks.Where(u => u.ReturnedDate != null);
        } 
        else if(status == "Unreturned")
        {
            userBooks = userBooks.Where(u => u.ReturnedDate == null);
        }
        return View(userBooks.ToPagedList(pageNumber, pageSize));
    }


}