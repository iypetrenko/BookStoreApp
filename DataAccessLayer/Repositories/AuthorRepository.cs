using Microsoft.EntityFrameworkCore;
using DomainTables.Models;
using DataAccessLayer.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccessLayer.Repositories
{
    public class AuthorRepository : Repository<Author>
    {
        public AuthorRepository(BookStoreContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Author>> GetAuthorsWithBooksAsync()
        {
            return await _dbSet.Include(a => a.Books).ToListAsync();
        }

        public async Task<Author> GetAuthorWithBooksAsync(int authorId)
        {
            return await _dbSet.Include(a => a.Books)
                              .FirstOrDefaultAsync(a => a.AuthorId == authorId);
        }
    }
}