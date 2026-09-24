using MemoLingo.Domain.Entities;
using MemoLingo.Domain.Repositories;
using MemoLingo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MemoLingo.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User> GetWithProgressesAsync(int id)
        {
            return await _context.Users
                .AsNoTracking()
                .Include(u => u.LanguageProgresses)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User> GetDefaultAsync()
        {
            return await _context.Users
                .AsNoTracking()
                .Include(u => u.LanguageProgresses)
                .Where(u => u.Active)
                .OrderBy(u => u.Id)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<LanguageProgress>> GetProgressesAsync(int userId)
        {
            return await _context.LanguageProgresses
                .AsNoTracking()
                .Include(lp => lp.Language)
                .Where(lp => lp.UserId == userId)
                .OrderByDescending(lp => lp.IsActiveCourse)
                .ThenBy(lp => lp.CreatedAt)
                .ToListAsync();
        }

        public async Task<LanguageProgress> GetProgressAsync(int userId, int languageId)
        {
            return await _context.LanguageProgresses
                .AsNoTracking()
                .Include(lp => lp.Language)
                .FirstOrDefaultAsync(lp => lp.UserId == userId && lp.LanguageId == languageId);
        }

        public async Task AddProgressAsync(LanguageProgress progress)
        {
            await _context.LanguageProgresses.AddAsync(progress);
            await _context.SaveChangesAsync();
        }

        public async Task SetActiveCourseAsync(int userId, int languageId)
        {
            var progresses = await _context.LanguageProgresses
                .Where(lp => lp.UserId == userId)
                .ToListAsync();

            foreach (var progress in progresses)
            {
                progress.IsActiveCourse = progress.LanguageId == languageId;
            }

            await _context.SaveChangesAsync();
        }
    }
}
