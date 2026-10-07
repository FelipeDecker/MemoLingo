using MemoLingo.Domain.Entities;
using MemoLingo.Domain.Enums;
using MemoLingo.Domain.Repositories;
using MemoLingo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MemoLingo.Infrastructure.Repositories
{
    public class SentenceRepository : ISentenceRepository
    {
        private readonly AppDbContext _context;

        public SentenceRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Sentence>> GetByLevelAsync(int languageId, CefrLevel cefrLevel)
        {
            return await _context.Sentences
                .AsNoTracking()
                .Include(s => s.SentenceWords)
                .Where(s => s.LanguageId == languageId && s.CefrLevel == cefrLevel)
                .AsSplitQuery()
                .ToListAsync();
        }

        public async Task<Sentence> GetByIdAsync(int id)
        {
            return await _context.Sentences
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);
        }
    }
}
