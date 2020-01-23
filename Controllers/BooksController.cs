using LibraryApi2.Domain;
using LibraryApi2.Models;
using Microsoft.AspNetCore.Http;
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

        /// <summary>
        /// Gives you all the books that are currently in inventory
        /// </summary>
        /// <returns>Those books.</returns>
        IQueryable<Book> GetBooksInInventory()
        {
            return _context.Books.Where(b => b.InInventory);
        }

        [HttpPut("/books/{id:int}/genre")]
        public async Task<IActionResult> UpdateTheGenre(int id, [FromBody] string genre)
        {
            var book = await GetBooksInInventory().SingleOrDefaultAsync(b => b.Id == id);
            if(book == null)
            {
                return NotFound();
            }
            else
            {
                book.Genre = genre;
                await _context.SaveChangesAsync();

                return NoContent();
            }
        }

        [HttpDelete("/books/{id:int}")]
        public async Task<IActionResult> RemoveBookFromInventory(int id)
        {
            var book = await GetBooksInInventory().SingleOrDefaultAsync(b => b.Id == id);

            if (book != null)
            {
                book.InInventory = false;
                await _context.SaveChangesAsync();
            }

            return NoContent();
        }

        /// <summary>
        /// Add a book to the inventory
        /// </summary>
        /// <param name="bookToAdd">Information about the book you want to add</param>
        /// <returns></returns>
        [HttpPost("/books")]
        [Produces("application/json")]
        public async Task<ActionResult<GetBookDetailsResponse>> AddABook([FromBody] PostBooksRequest bookToAdd)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var book = new Book
            {
                Title = bookToAdd.Title,
                Author = bookToAdd.Author,
                Genre = bookToAdd.Genre,
                NumberOfPages = bookToAdd.NumberOfPages,
                InInventory = true
            };

            _context.Books.Add(book);

            await _context.SaveChangesAsync();

            var response = new GetBookDetailsResponse
            {
                Id = book.Id,
                Title = book.Title,
                Author = book.Author,
                Genre = book.Genre,
                NumberOfPages = book.NumberOfPages
            };


            return CreatedAtRoute("books#getbookbyid", new { id = response.Id }, response);
        }

        /// <summary>
        /// Provides a list of all the books in our inventory.
        /// </summary>
        /// <param name="genre">If you'd like to be able to filter by genre, use this. Otherwise all books will be returned.</param>
        /// <returns>A list of books.</returns>
        /// <response code="200">Returns all of your books.</response>
        [HttpGet("/books")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<GetBooksResponse>> GetAllBooks([FromQuery] string genre = "all")
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

        [HttpGet("/books/{id:int}", Name = "books#getbookbyid")]
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
