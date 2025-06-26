using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DomainTables.Models;
using DataAccessLayer.Data;
using System.Runtime.InteropServices;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookReviewsController : ControllerBase
    {
        private readonly BookStoreContext _context;

        public BookReviewsController(BookStoreContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Получить все отзывы
        /// </summary>
        /// <returns>Список отзывов</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BookReview>>> GetBookReviews()
        {
            return await _context.BookReviews.Include(br => br.Book).ToListAsync();
        }

        /// <summary>
        /// Получить отзыв по ID
        /// </summary>
        /// <param name="id">ID отзыва</param>
        /// <returns>Отзыв с указанным ID</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<BookReview>> GetBookReview(int id)
        {
            var bookReview = await _context.BookReviews
                .Include(br => br.Book)
                .FirstOrDefaultAsync(br => br.ReviewId == id);

            if (bookReview == null)
            {
                return NotFound();
            }

            return bookReview;
        }

        /// <summary>
        /// Обновить отзыв
        /// </summary>
        /// <param name="id">ID отзыва</param>
        /// <param name="bookReview">Обновленные данные отзыва</param>
        /// <returns>Результат операции</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBookReview(int id, BookReview bookReview)
        {
            if (id != bookReview.ReviewId)
            {
                return BadRequest();
            }

            _context.Entry(bookReview).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BookReviewExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        /// <summary>
        /// Создать новый отзыв
        /// </summary>
        /// <param name="bookReview">Данные нового отзыва</param>
        /// <returns>Созданный отзыв</returns>
        [HttpPost]
        public async Task<ActionResult<BookReview>> PostBookReview(BookReview bookReview)
        {
            bookReview.ReviewDate = DateTime.Now;
            _context.BookReviews.Add(bookReview);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetBookReview", new { id = bookReview.ReviewId }, bookReview);
        }

        /// <summary>
        /// Удалить отзыв
        /// </summary>
        /// <param name="id">ID отзыва</param>
        /// <returns>Результат операции</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBookReview(int id)
        {
            var bookReview = await _context.BookReviews.FindAsync(id);
            if (bookReview == null)
            {
                return NotFound();
            }

            _context.BookReviews.Remove(bookReview);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Получить отзывы для конкретной книги
        /// </summary>
        /// <param name="bookId">ID книги</param>
        /// <returns>Список отзывов для книги</returns>
        [HttpGet("book/{bookId}")]
        public async Task<ActionResult<IEnumerable<BookReview>>> GetReviewsByBook(int bookId)
        {
            return await _context.BookReviews
                .Include(br => br.Book)
                .Where(br => br.BookId == bookId)
                .ToListAsync();
        }

        private bool BookReviewExists(int id)
        {
            return _context.BookReviews.Any(e => e.ReviewId == id);
        }
    }
}