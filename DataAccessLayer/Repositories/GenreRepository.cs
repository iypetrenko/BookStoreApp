using Microsoft.EntityFrameworkCore;
using DomainTables.Models;
using DataAccessLayer.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccessLayer.Repositories
{
    public class GenreRepository : Repository<Genre>
    {
        public GenreRepository(BookStoreContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Genre>> GetGenresWithBooksAsync()
        {
            return await _dbSet.Include(g => g.Books).ToListAsync();
        }
    }
}