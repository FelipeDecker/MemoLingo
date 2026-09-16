using MemoLingo.Domain.Entities;
using MemoLingo.Domain.Repositories;
using MemoLingo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MemoLingo.Infrastructure.Repositories
{
    public class CourseRepository : ICourseRepository
    {
        private readonly AppDbContext _context;

        public CourseRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Course>> GetActiveWithLessonsAsync(int? languageId)
        {
            var query = _context.Courses
                .AsNoTracking()
                .Where(c => c.Active);

            if (languageId.HasValue)
            {
                query = query.Where(c => c.LanguageId == languageId.Value);
            }

            return await query
                .OrderBy(c => c.Position)
                .Select(c => new Course
                {
                    Id = c.Id,
                    LanguageId = c.LanguageId,
                    Name = c.Name,
                    Description = c.Description,
                    Position = c.Position,
                    CefrLevel = c.CefrLevel,
                    Active = c.Active,
                    Lessons = c.Lessons
                        .Where(l => l.Active)
                        .OrderBy(l => l.Position)
                        .ToList()
                })
                .ToListAsync();
        }
    }
}
