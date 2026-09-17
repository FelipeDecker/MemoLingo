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

        public async Task<IEnumerable<Word>> GetLearnedByUserAsync(int userId, int languageId)
        {
            // Palavras que o usuário já praticou pelo menos uma vez.
            var practicedWordIds = _context.WordPerformances
                .Where(wp => wp.UserId == userId)
                .Select(wp => wp.WordId);

            // Palavras vistas em lições de nós da trilha já concluídos pelo usuário.
            var completedNodeWordIds = _context.LessonWords
                .Where(lw => _context.UserNodeProgresses.Any(unp =>
                    unp.UserId == userId
                    && unp.IsCompleted
                    && unp.PathNodeId == lw.Lesson.PathNodeId))
                .Select(lw => lw.WordId);

            return await _context.Words
                .AsNoTracking()
                .Where(w => w.LanguageId == languageId
                    && (practicedWordIds.Contains(w.Id) || completedNodeWordIds.Contains(w.Id)))
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
