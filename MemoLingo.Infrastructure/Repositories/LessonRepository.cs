using MemoLingo.Domain.Entities;
using MemoLingo.Domain.Repositories;
using MemoLingo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MemoLingo.Infrastructure.Repositories
{
    public class LessonRepository : ILessonRepository
    {
        private readonly AppDbContext _context;

        public LessonRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PathNode> GetNodeWithContextAsync(int pathNodeId)
        {
            return await _context.PathNodes
                .AsNoTrackingWithIdentityResolution()
                .Include(pn => pn.Lessons.Where(l => l.Active))
                .Include(pn => pn.Unit)
                    .ThenInclude(u => u.Section)
                        .ThenInclude(s => s.Course)
                .AsSplitQuery()
                .FirstOrDefaultAsync(pn => pn.Id == pathNodeId && pn.Active);
        }

        public async Task<Lesson> GetWithContextAsync(int lessonId)
        {
            return await _context.Lessons
                .AsNoTrackingWithIdentityResolution()
                .Include(l => l.PathNode)
                    .ThenInclude(pn => pn.Lessons.Where(nl => nl.Active))
                .Include(l => l.PathNode)
                    .ThenInclude(pn => pn.Unit)
                        .ThenInclude(u => u.Section)
                            .ThenInclude(s => s.Course)
                .AsSplitQuery()
                .FirstOrDefaultAsync(l => l.Id == lessonId);
        }

        public async Task<IEnumerable<LessonWord>> GetCourseLessonWordsAsync(int courseId)
        {
            return await _context.LessonWords
                .AsNoTracking()
                .Include(lw => lw.Word)
                .Include(lw => lw.Lesson)
                    .ThenInclude(l => l.PathNode)
                        .ThenInclude(pn => pn.Unit)
                            .ThenInclude(u => u.Section)
                .Where(lw => lw.Lesson.Active
                    && lw.Lesson.PathNode.Active
                    && lw.Lesson.PathNode.Unit.Active
                    && lw.Lesson.PathNode.Unit.Section.Active
                    && lw.Lesson.PathNode.Unit.Section.CourseId == courseId)
                .ToListAsync();
        }
    }
}
