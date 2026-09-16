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
    }
}
