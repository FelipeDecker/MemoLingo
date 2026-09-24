using MemoLingo.Domain.Entities;
using MemoLingo.Domain.Repositories;
using MemoLingo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MemoLingo.Infrastructure.Repositories
{
    public class LanguageRepository : ILanguageRepository
    {
        private readonly AppDbContext _context;

        public LanguageRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Language>> GetAllAsync()
        {
            return await _context.Languages
                .AsNoTracking()
                .OrderBy(l => l.Name)
                .ToListAsync();
        }

        public async Task<Language> GetByIdAsync(int id)
        {
            return await _context.Languages
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.Id == id);
        }
    }
}
