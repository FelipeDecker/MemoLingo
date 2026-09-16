using MemoLingo.Domain.Entities;
using MemoLingo.Domain.Repositories;
using MemoLingo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MemoLingo.Infrastructure.Repositories
{
    public class WordRepository : IWordRepository
    {
        private readonly AppDbContext _context;

        public WordRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Word>> GetByLanguageAsync(int languageId)
        {
            return await _context.Words
                .AsNoTracking()
                .Where(w => w.LanguageId == languageId)
                .OrderBy(w => w.CefrLevel)
                .ThenBy(w => w.Text)
                .ToListAsync();
        }

        public async Task<Word> GetByIdAsync(int id)
        {
            return await _context.Words
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.Id == id);
        }
    }
}
