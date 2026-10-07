using MemoLingo.Domain.Entities;
using MemoLingo.Domain.Repositories;
using MemoLingo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MemoLingo.Infrastructure.Repositories
{
    public class UserNodeProgressRepository : IUserNodeProgressRepository
    {
        private readonly AppDbContext _context;

        public UserNodeProgressRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<UserNodeProgress> GetAsync(int userId, int pathNodeId)
        {
            return await _context.UserNodeProgresses
                .FirstOrDefaultAsync(unp => unp.UserId == userId && unp.PathNodeId == pathNodeId);
        }

        public async Task AddAsync(UserNodeProgress progress)
        {
            await _context.UserNodeProgresses.AddAsync(progress);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
