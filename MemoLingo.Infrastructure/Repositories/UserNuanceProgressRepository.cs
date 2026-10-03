using MemoLingo.Domain.Entities;
using MemoLingo.Domain.Repositories;
using MemoLingo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MemoLingo.Infrastructure.Repositories
{
    public class UserNuanceProgressRepository : IUserNuanceProgressRepository
    {
        private readonly AppDbContext _context;

        public UserNuanceProgressRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<UserNuanceProgress>> GetByUserAsync(int userId)
        {
            return await _context.UserNuanceProgresses
                .AsNoTracking()
                .Where(unp => unp.UserId == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<UserNuanceProgress>> GetByUserAndGroupsAsync(int userId, IEnumerable<int> groupIds)
        {
            var ids = groupIds.ToList();

            return await _context.UserNuanceProgresses
                .Where(unp => unp.UserId == userId && ids.Contains(unp.SynonymGroupId))
                .ToListAsync();
        }

        public async Task AddAsync(UserNuanceProgress progress)
        {
            await _context.UserNuanceProgresses.AddAsync(progress);
        }

        public void Update(UserNuanceProgress progress)
        {
            _context.UserNuanceProgresses.Update(progress);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
