using MemoLingo.Domain.Entities;
using MemoLingo.Domain.Repositories;
using MemoLingo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MemoLingo.Infrastructure.Repositories
{
    public class SynonymGroupRepository : ISynonymGroupRepository
    {
        private readonly AppDbContext _context;

        public SynonymGroupRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SynonymGroup>> GetWithExercisesByLanguageAsync(int languageId)
        {
            return await _context.SynonymGroups
                .AsNoTracking()
                .Where(sg => sg.LanguageId == languageId && sg.Exercises.Any())
                .Include(sg => sg.Items)
                    .ThenInclude(sgi => sgi.Word)
                .Include(sg => sg.Exercises)
                .AsSplitQuery()
                .ToListAsync();
        }

        public async Task<NuanceExercise> GetExerciseWithGroupAsync(int exerciseId)
        {
            return await _context.NuanceExercises
                .AsNoTrackingWithIdentityResolution()
                .Include(ne => ne.TargetWord)
                .Include(ne => ne.SynonymGroup)
                    .ThenInclude(sg => sg.Items)
                        .ThenInclude(sgi => sgi.Word)
                .Include(ne => ne.SynonymGroup)
                    .ThenInclude(sg => sg.Exercises)
                //.AsSplitQuery()
                .FirstOrDefaultAsync(ne => ne.Id == exerciseId);
        }

        public async Task<IEnumerable<int>> GetGroupIdsByWordAsync(int wordId)
        {
            return await _context.SynonymGroupItems
                .AsNoTracking()
                .Where(sgi => sgi.WordId == wordId)
                .Select(sgi => sgi.SynonymGroupId)
                .Distinct()
                .ToListAsync();
        }
    }
}
