using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DomainTables.Models;
using DataAccessLayer.Data;
using System.Runtime.InteropServices;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GenresController : ControllerBase
    {
        private readonly BookStoreContext _context;

        public GenresController(BookStoreContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Получить все жанры
        /// </summary>
        /// <returns>Список жанров</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Genre>>> GetGenres()
        {
            return await _context.Genres.Include(g => g.Books).ToListAsync();
        }

        /// <summary>
        /// Получить жанр по ID
        /// </summary>
        /// <param name="id">ID жанра</param>
        /// <returns>Жанр с указанным ID</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Genre>> GetGenre(int id)
        {
            var genre = await _context.Genres
                .Include(g => g.Books)
                .FirstOrDefaultAsync(g => g.GenreId == id);

            if (genre == null)
            {
                return NotFound();
            }

            return genre;
        }

        /// <summary>
        /// Обновить данные жанра
        /// </summary>
        /// <param name="id">ID жанра</param>
        /// <param name="genre">Обновленные данные жанра</param>
        /// <returns>Результат операции</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutGenre(int id, Genre genre)
        {
            if (id != genre.GenreId)
            {
                return BadRequest();
            }

            _context.Entry(genre).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GenreExists(id))
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
        /// Создать новый жанр
        /// </summary>
        /// <param name="genre">Данные нового жанра</param>
        /// <returns>Созданный жанр</returns>
        [HttpPost]
        public async Task<ActionResult<Genre>> PostGenre(Genre genre)
        {
            _context.Genres.Add(genre);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetGenre", new { id = genre.GenreId }, genre);
        }

        /// <summary>
        /// Удалить жанр
        /// </summary>
        /// <param name="id">ID жанра</param>
        /// <returns>Результат операции</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGenre(int id)
        {
            var genre = await _context.Genres.FindAsync(id);
            if (genre == null)
            {
                return NotFound();
            }

            // Проверяем наличие связанных книг
            var hasBooks = await _context.Books.AnyAsync(b => b.GenreId == id);
            if (hasBooks)
            {
                return BadRequest("Нельзя удалить жанр, который используется в книгах");
            }

            _context.Genres.Remove(genre);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool GenreExists(int id)
        {
            return _context.Genres.Any(e => e.GenreId == id);
        }
    }
}