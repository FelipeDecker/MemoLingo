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

        public async Task<IEnumerable<Course>> GetActiveTrackAsync(int? languageId)
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
                    Sections = c.Sections
                        .Where(s => s.Active)
                        .OrderBy(s => s.Position)
                        .Select(s => new Section
                        {
                            Id = s.Id,
                            CourseId = s.CourseId,
                            Title = s.Title,
                            Description = s.Description,
                            Position = s.Position,
                            CefrLevel = s.CefrLevel,
                            Active = s.Active,
                            Units = s.Units
                                .Where(u => u.Active)
                                .OrderBy(u => u.Position)
                                .Select(u => new Unit
                                {
                                    Id = u.Id,
                                    SectionId = u.SectionId,
                                    Title = u.Title,
                                    Topic = u.Topic,
                                    GuidebookMarkdown = u.GuidebookMarkdown,
                                    Position = u.Position,
                                    Active = u.Active,
                                    PathNodes = u.PathNodes
                                        .Where(pn => pn.Active)
                                        .OrderBy(pn => pn.Position)
                                        .Select(pn => new PathNode
                                        {
                                            Id = pn.Id,
                                            UnitId = pn.UnitId,
                                            NodeType = pn.NodeType,
                                            Position = pn.Position,
                                            TotalLessons = pn.TotalLessons,
                                            Active = pn.Active,
                                            Lessons = pn.Lessons
                                                .Where(l => l.Active)
                                                .OrderBy(l => l.Position)
                                                .ToList()
                                        })
                                        .ToList()
                                })
                                .ToList()
                        })
                        .ToList()
                })
                .ToListAsync();
        }
    }
}
