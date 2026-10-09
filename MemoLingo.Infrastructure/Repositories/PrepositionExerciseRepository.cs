using MemoLingo.Domain.Entities;
using MemoLingo.Domain.Enums;
using MemoLingo.Domain.Repositories;
using MemoLingo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MemoLingo.Infrastructure.Repositories
{
    public class PrepositionExerciseRepository : IPrepositionExerciseRepository
    {
        private readonly AppDbContext _context;

        public PrepositionExerciseRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PrepositionExercise>> GetByLanguageAsync(int languageId, PrepositionExerciseType? exerciseType)
        {
            var query = _context.PrepositionExercises
                .AsNoTracking()
                .Where(pe => pe.LanguageId == languageId);

            if (exerciseType.HasValue)
            {
                query = query.Where(pe => pe.ExerciseType == exerciseType.Value);
            }

            return await query.ToListAsync();
        }

        public async Task<PrepositionExercise> GetByIdAsync(int id)
        {
            return await _context.PrepositionExercises
                .AsNoTracking()
                .FirstOrDefaultAsync(pe => pe.Id == id);
        }
    }
}
