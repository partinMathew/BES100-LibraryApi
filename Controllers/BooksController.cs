using LibraryApi2.Domain;
using LibraryApi2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryApi2.Controllers
{
    public class BooksController : Controller
    {

        LibraryDataContext _context;

        public BooksController(LibraryDataContext context)
        {
            _context = context;
        }

        IQueryable<Book> GetBooksInInventory()
        {
            return _context.Books.Where(b => b.InInventory);
        }

        [HttpGet("/books")]
        public async Task<IActionResult> GetAllBooks([FromQuery] string genre = "all")
        {

            var books = GetBooksInInventory();

            if(genre != "all")
            {
                books = books.Where(b => b.Genre.ToLower() == genre.ToLower());
            }

            var booksListItem = await books.Select(b => new BookSummaryItem
            {
                Id = b.Id,
                Title = b.Title,
                Author = b.Author,
                Genre = b.Genre
            }).ToListAsync();
           
            var response = new GetBooksResponse 
            {
                Data = booksListItem, 
                Genre = genre, 
                Count = booksListItem.Count() 
            };

            return Ok(response);
        }

        [HttpGet("/books/{id:int}")]
        public async Task<IActionResult> GetBookById(int id)
        {
            var response = await GetBooksInInventory().Where(b => b.Id == id)
                .Select(b => new GetBookDetailsResponse
                {
                    Id = b.Id,
                    Title = b.Title,
                    Author = b.Author,
                    Genre = b.Genre,
                    NumberOfPages = b.NumberOfPages
                }).SingleOrDefaultAsync();

            if(response == null)
            {
                return NotFound("No book with that id!");
            }
            else
            {
                return Ok(response);
            }
        }
    }
}
