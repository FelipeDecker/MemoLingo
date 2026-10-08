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
                    && ss.SectionId == null
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

        public async Task<StudySession> GetByIdAsync(int id)
        {
            return await _context.StudySessions
                .FirstOrDefaultAsync(ss => ss.Id == id);
        }

        public async Task<StudySession> StartLessonSessionAsync(int userId, int languageId, int lessonId)
        {
            // Uma nova tentativa da lição descarta as sessões que ficaram abertas (ex.: o usuário
            // fechou a atividade no meio), para que os contadores sempre comecem do zero.
            var openSessions = await _context.StudySessions
                .Where(ss => ss.UserId == userId
                    && ss.LessonId == lessonId
                    && ss.Status == ProgressStatus.InProgress)
                .ToListAsync();

            var now = DateTime.UtcNow;

            foreach (var open in openSessions)
            {
                open.Status = ProgressStatus.Abandoned;
                open.FinishedAt = now;
            }

            var session = new StudySession
            {
                UserId = userId,
                LanguageId = languageId,
                LessonId = lessonId,
                Status = ProgressStatus.InProgress,
                StartedAt = now
            };

            await _context.StudySessions.AddAsync(session);
            await _context.SaveChangesAsync();

            return session;
        }

        public async Task<StudySession> StartSectionTestSessionAsync(int userId, int languageId, int sectionId, int exerciseCount)
        {
            // Um novo teste descarta tentativas anteriores que ficaram abertas para a mesma seção.
            var openSessions = await _context.StudySessions
                .Where(ss => ss.UserId == userId
                    && ss.SectionId == sectionId
                    && ss.Status == ProgressStatus.InProgress)
                .ToListAsync();

            var now = DateTime.UtcNow;

            foreach (var open in openSessions)
            {
                open.Status = ProgressStatus.Abandoned;
                open.FinishedAt = now;
            }

            var session = new StudySession
            {
                UserId = userId,
                LanguageId = languageId,
                SectionId = sectionId,
                ExerciseCount = exerciseCount,
                Status = ProgressStatus.InProgress,
                StartedAt = now
            };

            await _context.StudySessions.AddAsync(session);
            await _context.SaveChangesAsync();

            return session;
        }

        public async Task AddSkippedLessonSessionsAsync(int userId, int languageId, IEnumerable<int> lessonIds, DateTime completedAt)
        {
            // As lições puladas contam como concluídas na trilha, mas sem XP nem respostas registradas.
            var sessions = lessonIds
                .Distinct()
                .Select(lessonId => new StudySession
                {
                    UserId = userId,
                    LanguageId = languageId,
                    LessonId = lessonId,
                    Status = ProgressStatus.Completed,
                    StartedAt = completedAt,
                    FinishedAt = completedAt
                })
                .ToList();

            if (sessions.Count == 0)
            {
                return;
            }

            await _context.StudySessions.AddRangeAsync(sessions);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
