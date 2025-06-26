using Microsoft.EntityFrameworkCore;
using DomainTables.Models;
using DataAccessLayer.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccessLayer.Repositories
{
    public class BookReviewRepository : Repository<BookReview>
    {
        public BookReviewRepository(BookStoreContext context) : base(context)
        {
        }

        public async Task<IEnumerable<BookReview>> GetReviewsWithBookAsync()
        {
            return await _dbSet.Include(br => br.Book).ToListAsync();
        }

        public async Task<IEnumerable<BookReview>> GetReviewsByBookIdAsync(int bookId)
        {
            return await _dbSet.Include(br => br.Book)
                              .Where(br => br.BookId == bookId)
                              .ToListAsync();
        }
    }
}