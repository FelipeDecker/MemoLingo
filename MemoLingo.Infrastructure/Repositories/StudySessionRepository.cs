using MemoLingo.Domain.Enums;
using MemoLingo.Domain.Repositories;
using MemoLingo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MemoLingo.Infrastructure.Repositories
{
    public class StudySessionRepository : IStudySessionRepository
    {
        private readonly AppDbContext _context;

        public StudySessionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<int>> GetCompletedLessonIdsAsync(int userId)
        {
            return await _context.StudySessions
                .AsNoTracking()
                .Where(ss => ss.UserId == userId
                    && ss.LessonId.HasValue
                    && ss.Status == ProgressStatus.Completed)
                .Select(ss => ss.LessonId.Value)
                .Distinct()
                .ToListAsync();
        }

        public async Task<IEnumerable<int>> GetInProgressLessonIdsAsync(int userId)
        {
            return await _context.StudySessions
                .AsNoTracking()
                .Where(ss => ss.UserId == userId
                    && ss.LessonId.HasValue
                    && ss.Status == ProgressStatus.InProgress)
                .Select(ss => ss.LessonId.Value)
                .Distinct()
                .ToListAsync();
        }
    }
}
