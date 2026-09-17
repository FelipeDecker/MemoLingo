using MemoLingo.Domain.Entities;
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

        public async Task<StudySession> GetOrCreatePracticeSessionAsync(int userId, int languageId)
        {
            // As sessões de prática livre não têm lição associada; reaproveitamos a
            // sessão aberta do usuário para agrupar as tentativas do mesmo dia.
            var session = await _context.StudySessions
                .Where(ss => ss.UserId == userId
                    && ss.LanguageId == languageId
                    && ss.LessonId == null
                    && ss.Status == ProgressStatus.InProgress)
                .OrderByDescending(ss => ss.StartedAt)
                .FirstOrDefaultAsync();

            if (session is not null)
            {
                return session;
            }

            session = new StudySession
            {
                UserId = userId,
                LanguageId = languageId,
                Status = ProgressStatus.InProgress,
                StartedAt = DateTime.UtcNow
            };

            await _context.StudySessions.AddAsync(session);
            await _context.SaveChangesAsync();

            return session;
        }
    }
}
