using MemoLingo.Domain.Entities;
using MemoLingo.Domain.Repositories;
using MemoLingo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MemoLingo.Infrastructure.Repositories
{
    public class WordPerformanceRepository : IWordPerformanceRepository
    {
        private readonly AppDbContext _context;

        public WordPerformanceRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<WordPerformance>> GetByUserAsync(int userId)
        {
            return await _context.WordPerformances
                .AsNoTracking()
                .Where(wp => wp.UserId == userId)
                .ToListAsync();
        }

        public async Task<WordPerformance> GetByUserAndWordAsync(int userId, int wordId)
        {
            return await _context.WordPerformances
                .FirstOrDefaultAsync(wp => wp.UserId == userId && wp.WordId == wordId);
        }

        public async Task AddAsync(WordPerformance performance)
        {
            await _context.WordPerformances.AddAsync(performance);
        }

        public void Update(WordPerformance performance)
        {
            _context.WordPerformances.Update(performance);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
