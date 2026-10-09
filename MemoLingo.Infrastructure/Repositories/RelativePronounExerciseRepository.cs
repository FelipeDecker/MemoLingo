using MemoLingo.Domain.Entities;
using MemoLingo.Domain.Enums;
using MemoLingo.Domain.Repositories;
using MemoLingo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MemoLingo.Infrastructure.Repositories
{
    public class RelativePronounExerciseRepository : IRelativePronounExerciseRepository
    {
        private readonly AppDbContext _context;

        public RelativePronounExerciseRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<RelativePronounExercise>> GetByLanguageAsync(int languageId, RelativePronounExerciseType? exerciseType)
        {
            var query = _context.RelativePronounExercises
                .AsNoTracking()
                .Where(re => re.LanguageId == languageId);

            if (exerciseType.HasValue)
            {
                query = query.Where(re => re.ExerciseType == exerciseType.Value);
            }

            return await query.ToListAsync();
        }

        public async Task<RelativePronounExercise> GetByIdAsync(int id)
        {
            return await _context.RelativePronounExercises
                .AsNoTracking()
                .FirstOrDefaultAsync(re => re.Id == id);
        }
    }
}
