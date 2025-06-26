using Microsoft.EntityFrameworkCore;
using DomainTables.Models;
using DataAccessLayer.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccessLayer.Repositories
{
    public class BookRepository : Repository<Book>
    {
        public BookRepository(BookStoreContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Book>> GetBooksWithDetailsAsync()
        {
            return await _dbSet.Include(b => b.Author)
                              .Include(b => b.Genre)
                              .Include(b => b.BookReviews)
                              .ToListAsync();
        }

        public async Task<Book> GetBookWithDetailsAsync(int bookId)
        {
            return await _dbSet.Include(b => b.Author)
                              .Include(b => b.Genre)
                              .Include(b => b.BookReviews)
                              .FirstOrDefaultAsync(b => b.BookId == bookId);
        }
    }
}