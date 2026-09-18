using System.Text.Json;
using System.Text.Json.Serialization;
using MemoLingo.Domain.Entities;
using MemoLingo.Domain.Enums;
using MemoLingo.Infrastructure.Data.Seeding.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MemoLingo.Infrastructure.Data.Seeding
{
    public class DatabaseSeeder
    {
        private const string DefaultSeedPath = "Seed";

        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
            Converters = { new JsonStringEnumConverter() }
        };

        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<DatabaseSeeder> _logger;

        public DatabaseSeeder(AppDbContext context, IConfiguration configuration, ILogger<DatabaseSeeder> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SeedAsync(CancellationToken cancellationToken = default)
        {
            var seedPath = ResolveSeedPath();

            if (!Directory.Exists(seedPath))
            {
                _logger.LogWarning("Diretório de seed não encontrado em {SeedPath}. Nenhum dado foi carregado.", seedPath);
                return;
            }

            _logger.LogInformation("Carregando dados de seed a partir de {SeedPath}.", seedPath);

            var languages = await SeedLanguagesAsync(seedPath, cancellationToken);
            var words = await SeedWordsAsync(seedPath, languages, cancellationToken);
            var sentences = await SeedSentencesAsync(seedPath, languages, cancellationToken);
            await SeedSentenceWordsAsync(seedPath, words, sentences, cancellationToken);
            var courses = await SeedCoursesAsync(seedPath, languages, cancellationToken);
            var sections = await SeedSectionsAsync(seedPath, languages, courses, cancellationToken);
            var units = await SeedUnitsAsync(seedPath, languages, courses, sections, cancellationToken);
            var nodes = await SeedPathNodesAsync(seedPath, languages, courses, units, cancellationToken);
            var lessons = await SeedLessonsAsync(seedPath, languages, courses, units, nodes, cancellationToken);
            await SeedLessonWordsAsync(seedPath, languages, courses, units, nodes, lessons, words, cancellationToken);

            // O seed carrega apenas o conteúdo do curso. Nenhum progresso do usuário
            // (UserNodeProgress, StudySession, ExerciseAttempt) é inserido: o mapa começa
            // totalmente bloqueado e o histórico de tentativas nasce vazio.
            await SeedUsersAsync(seedPath, languages, cancellationToken);

            _logger.LogInformation("Seed concluído.");
        }

        private string ResolveSeedPath()
        {
            var configuredPath = _configuration["Seed:Path"];
            var path = string.IsNullOrWhiteSpace(configuredPath) ? DefaultSeedPath : configuredPath;

            return Path.IsPathRooted(path) ? path : Path.Combine(AppContext.BaseDirectory, path);
        }

        private async Task<List<T>> ReadAsync<T>(string seedPath, string fileName, CancellationToken cancellationToken)
        {
            var filePath = Path.Combine(seedPath, fileName);

            if (!File.Exists(filePath))
            {
                _logger.LogWarning("Arquivo de seed {FileName} não encontrado.", fileName);
                return new List<T>();
            }

            await using var stream = File.OpenRead(filePath);
            var items = await JsonSerializer.DeserializeAsync<List<T>>(stream, SerializerOptions, cancellationToken);

            return items ?? new List<T>();
        }

        private async Task<Dictionary<string, int>> SeedLanguagesAsync(string seedPath, CancellationToken cancellationToken)
        {
            var seeds = await ReadAsync<LanguageSeed>(seedPath, "languages.json", cancellationToken);

            var existing = await _context.Languages
                .ToDictionaryAsync(l => l.Code.ToLowerInvariant(), l => l.Id, cancellationToken);

            var created = 0;

            foreach (var seed in seeds)
            {
                if (existing.ContainsKey(seed.Code.ToLowerInvariant()))
                {
                    continue;
                }

                _context.Languages.Add(new Language { Code = seed.Code, Name = seed.Name });
                created++;
            }

            if (created > 0)
            {
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("{Count} idioma(s) inserido(s).", created);
            }

            return await _context.Languages
                .ToDictionaryAsync(l => l.Code.ToLowerInvariant(), l => l.Id, cancellationToken);
        }

        private async Task<Dictionary<(int LanguageId, string Text), int>> SeedWordsAsync(
            string seedPath,
            Dictionary<string, int> languages,
            CancellationToken cancellationToken)
        {
            var seeds = await ReadAsync<WordSeed>(seedPath, "words.json", cancellationToken);

            var existing = await LoadWordsAsync(cancellationToken);
            var created = 0;

            foreach (var seed in seeds)
            {
                if (!languages.TryGetValue(seed.LanguageCode.ToLowerInvariant(), out var languageId))
                {
                    _logger.LogWarning("Idioma {LanguageCode} não encontrado para a palavra {Word}.", seed.LanguageCode, seed.Text);
                    continue;
                }

                if (existing.ContainsKey((languageId, seed.Text.ToLowerInvariant())))
                {
                    continue;
                }

                _context.Words.Add(new Word
                {
                    LanguageId = languageId,
                    Text = seed.Text,
                    Translation = seed.Translation,
                    CefrLevel = seed.CefrLevel,
                    PartOfSpeech = seed.PartOfSpeech
                });

                created++;
            }

            if (created > 0)
            {
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("{Count} palavra(s) inserida(s).", created);
            }

            return await LoadWordsAsync(cancellationToken);
        }

        private async Task<Dictionary<(int LanguageId, string Text), int>> LoadWordsAsync(CancellationToken cancellationToken)
        {
            var words = await _context.Words
                .Select(w => new { w.Id, w.LanguageId, w.Text })
                .ToListAsync(cancellationToken);

            return words.ToDictionary(w => (w.LanguageId, w.Text.ToLowerInvariant()), w => w.Id);
        }

        private async Task<Dictionary<string, (int Id, int LanguageId)>> SeedSentencesAsync(
            string seedPath,
            Dictionary<string, int> languages,
            CancellationToken cancellationToken)
        {
            var seeds = await ReadAsync<SentenceSeed>(seedPath, "sentences.json", cancellationToken);

            var existing = await LoadSentencesAsync(cancellationToken);
            var created = 0;

            foreach (var seed in seeds)
            {
                if (!languages.TryGetValue(seed.LanguageCode.ToLowerInvariant(), out var languageId))
                {
                    _logger.LogWarning("Idioma {LanguageCode} não encontrado para a frase {Sentence}.", seed.LanguageCode, seed.Text);
                    continue;
                }

                if (existing.ContainsKey(seed.Text.ToLowerInvariant()))
                {
                    continue;
                }

                _context.Sentences.Add(new Sentence
                {
                    LanguageId = languageId,
                    Text = seed.Text,
                    Translation = seed.Translation,
                    CefrLevel = seed.CefrLevel
                });

                created++;
            }

            if (created > 0)
            {
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("{Count} frase(s) inserida(s).", created);
            }

            return await LoadSentencesAsync(cancellationToken);
        }

        private async Task<Dictionary<string, (int Id, int LanguageId)>> LoadSentencesAsync(CancellationToken cancellationToken)
        {
            var sentences = await _context.Sentences
                .Select(s => new { s.Id, s.LanguageId, s.Text })
                .ToListAsync(cancellationToken);

            return sentences
                .GroupBy(s => s.Text.ToLowerInvariant())
                .ToDictionary(g => g.Key, g => (g.First().Id, g.First().LanguageId));
        }

        private async Task SeedSentenceWordsAsync(
            string seedPath,
            Dictionary<(int LanguageId, string Text), int> words,
            Dictionary<string, (int Id, int LanguageId)> sentences,
            CancellationToken cancellationToken)
        {
            var seeds = await ReadAsync<SentenceWordSeed>(seedPath, "sentence-words.json", cancellationToken);

            var existing = (await _context.SentenceWords
                .Select(sw => new { sw.SentenceId, sw.WordId })
                .ToListAsync(cancellationToken))
                .Select(sw => (sw.SentenceId, sw.WordId))
                .ToHashSet();

            var created = 0;

            foreach (var seed in seeds)
            {
                if (!sentences.TryGetValue(seed.SentenceText.ToLowerInvariant(), out var sentence))
                {
                    _logger.LogWarning("Frase não encontrada para o vínculo: {Sentence}.", seed.SentenceText);
                    continue;
                }

                if (!words.TryGetValue((sentence.LanguageId, seed.WordText.ToLowerInvariant()), out var wordId))
                {
                    _logger.LogWarning("Palavra {Word} não encontrada para a frase {Sentence}.", seed.WordText, seed.SentenceText);
                    continue;
                }

                if (!existing.Add((sentence.Id, wordId)))
                {
                    continue;
                }

                _context.SentenceWords.Add(new SentenceWord
                {
                    SentenceId = sentence.Id,
                    WordId = wordId,
                    Position = seed.Position
                });

                created++;
            }

            if (created > 0)
            {
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("{Count} vínculo(s) frase/palavra inserido(s).", created);
            }
        }

        private async Task<Dictionary<(int LanguageId, string Name), int>> SeedCoursesAsync(
            string seedPath,
            Dictionary<string, int> languages,
            CancellationToken cancellationToken)
        {
            var seeds = await ReadAsync<CourseSeed>(seedPath, "courses.json", cancellationToken);

            var existing = await LoadCoursesAsync(cancellationToken);
            var created = 0;

            foreach (var seed in seeds)
            {
                if (!languages.TryGetValue(seed.LanguageCode.ToLowerInvariant(), out var languageId))
                {
                    _logger.LogWarning("Idioma {LanguageCode} não encontrado para o curso {Course}.", seed.LanguageCode, seed.Name);
                    continue;
                }

                if (existing.ContainsKey((languageId, seed.Name.ToLowerInvariant())))
                {
                    continue;
                }

                _context.Courses.Add(new Course
                {
                    LanguageId = languageId,
                    Name = seed.Name,
                    Description = seed.Description,
                    Position = seed.Position,
                    CefrLevel = seed.CefrLevel,
                    Active = seed.Active
                });

                created++;
            }

            if (created > 0)
            {
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("{Count} curso(s) inserido(s).", created);
            }

            return await LoadCoursesAsync(cancellationToken);
        }

        private async Task<Dictionary<(int LanguageId, string Name), int>> LoadCoursesAsync(CancellationToken cancellationToken)
        {
            var courses = await _context.Courses
                .Select(c => new { c.Id, c.LanguageId, c.Name })
                .ToListAsync(cancellationToken);

            return courses.ToDictionary(c => (c.LanguageId, c.Name.ToLowerInvariant()), c => c.Id);
        }

        private async Task<Dictionary<(int CourseId, string Title), int>> SeedSectionsAsync(
            string seedPath,
            Dictionary<string, int> languages,
            Dictionary<(int LanguageId, string Name), int> courses,
            CancellationToken cancellationToken)
        {
            var seeds = await ReadAsync<SectionSeed>(seedPath, "sections.json", cancellationToken);

            var existing = await LoadSectionsAsync(cancellationToken);
            var created = 0;

            foreach (var seed in seeds)
            {
                if (!TryResolveCourse(languages, courses, seed.LanguageCode, seed.CourseName, out var courseId))
                {
                    _logger.LogWarning("Curso {Course} não encontrado para a seção {Section}.", seed.CourseName, seed.Title);
                    continue;
                }

                if (existing.ContainsKey((courseId, seed.Title.ToLowerInvariant())))
                {
                    continue;
                }

                _context.Sections.Add(new Section
                {
                    CourseId = courseId,
                    Title = seed.Title,
                    Description = seed.Description,
                    Position = seed.Position,
                    CefrLevel = seed.CefrLevel,
                    Active = seed.Active
                });

                created++;
            }

            if (created > 0)
            {
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("{Count} seção(ões) inserida(s).", created);
            }

            return await LoadSectionsAsync(cancellationToken);
        }

        private async Task<Dictionary<(int CourseId, string Title), int>> LoadSectionsAsync(CancellationToken cancellationToken)
        {
            var sections = await _context.Sections
                .Select(s => new { s.Id, s.CourseId, s.Title })
                .ToListAsync(cancellationToken);

            return sections.ToDictionary(s => (s.CourseId, s.Title.ToLowerInvariant()), s => s.Id);
        }

        private async Task<Dictionary<(int CourseId, string Title), int>> SeedUnitsAsync(
            string seedPath,
            Dictionary<string, int> languages,
            Dictionary<(int LanguageId, string Name), int> courses,
            Dictionary<(int CourseId, string Title), int> sections,
            CancellationToken cancellationToken)
        {
            var seeds = await ReadAsync<UnitSeed>(seedPath, "units.json", cancellationToken);

            var existing = await LoadUnitsAsync(cancellationToken);
            var created = 0;

            foreach (var seed in seeds)
            {
                if (!TryResolveCourse(languages, courses, seed.LanguageCode, seed.CourseName, out var courseId))
                {
                    _logger.LogWarning("Curso {Course} não encontrado para a unidade {Unit}.", seed.CourseName, seed.Title);
                    continue;
                }

                if (!sections.TryGetValue((courseId, seed.SectionTitle.ToLowerInvariant()), out var sectionId))
                {
                    _logger.LogWarning("Seção {Section} não encontrada para a unidade {Unit}.", seed.SectionTitle, seed.Title);
                    continue;
                }

                if (existing.ContainsKey((courseId, seed.Title.ToLowerInvariant())))
                {
                    continue;
                }

                _context.Units.Add(new Unit
                {
                    SectionId = sectionId,
                    Title = seed.Title,
                    Topic = seed.Topic,
                    GuidebookMarkdown = seed.GuidebookMarkdown,
                    Position = seed.Position,
                    Active = seed.Active
                });

                created++;
            }

            if (created > 0)
            {
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("{Count} unidade(s) inserida(s).", created);
            }

            return await LoadUnitsAsync(cancellationToken);
        }

        private async Task<Dictionary<(int CourseId, string Title), int>> LoadUnitsAsync(CancellationToken cancellationToken)
        {
            var units = await _context.Units
                .Select(u => new { u.Id, u.Section.CourseId, u.Title })
                .ToListAsync(cancellationToken);

            return units.ToDictionary(u => (u.CourseId, u.Title.ToLowerInvariant()), u => u.Id);
        }

        private async Task<Dictionary<(int UnitId, int Position), int>> SeedPathNodesAsync(
            string seedPath,
            Dictionary<string, int> languages,
            Dictionary<(int LanguageId, string Name), int> courses,
            Dictionary<(int CourseId, string Title), int> units,
            CancellationToken cancellationToken)
        {
            var seeds = await ReadAsync<PathNodeSeed>(seedPath, "path-nodes.json", cancellationToken);

            var existing = await LoadPathNodesAsync(cancellationToken);
            var created = 0;

            foreach (var seed in seeds)
            {
                if (!TryResolveUnit(languages, courses, units, seed.LanguageCode, seed.CourseName, seed.UnitTitle, out var unitId))
                {
                    _logger.LogWarning("Unidade {Unit} não encontrada para o nó de posição {Position}.", seed.UnitTitle, seed.Position);
                    continue;
                }

                if (existing.ContainsKey((unitId, seed.Position)))
                {
                    continue;
                }

                _context.PathNodes.Add(new PathNode
                {
                    UnitId = unitId,
                    NodeType = seed.NodeType,
                    Position = seed.Position,
                    TotalLessons = seed.TotalLessons,
                    Active = seed.Active
                });

                created++;
            }

            if (created > 0)
            {
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("{Count} nó(s) de trilha inserido(s).", created);
            }

            return await LoadPathNodesAsync(cancellationToken);
        }

        private async Task<Dictionary<(int UnitId, int Position), int>> LoadPathNodesAsync(CancellationToken cancellationToken)
        {
            var nodes = await _context.PathNodes
                .Select(pn => new { pn.Id, pn.UnitId, pn.Position })
                .ToListAsync(cancellationToken);

            return nodes.ToDictionary(pn => (pn.UnitId, pn.Position), pn => pn.Id);
        }

        private async Task<Dictionary<(int PathNodeId, int Position), int>> SeedLessonsAsync(
            string seedPath,
            Dictionary<string, int> languages,
            Dictionary<(int LanguageId, string Name), int> courses,
            Dictionary<(int CourseId, string Title), int> units,
            Dictionary<(int UnitId, int Position), int> nodes,
            CancellationToken cancellationToken)
        {
            var seeds = await ReadAsync<LessonSeed>(seedPath, "lessons.json", cancellationToken);

            var existing = await LoadLessonsAsync(cancellationToken);
            var created = 0;

            foreach (var seed in seeds)
            {
                if (!TryResolveNode(languages, courses, units, nodes, seed.LanguageCode, seed.CourseName, seed.UnitTitle, seed.NodePosition, out var pathNodeId))
                {
                    _logger.LogWarning("Nó {Node} da unidade {Unit} não encontrado para a lição.", seed.NodePosition, seed.UnitTitle);
                    continue;
                }

                if (existing.ContainsKey((pathNodeId, seed.Position)))
                {
                    continue;
                }

                _context.Lessons.Add(new Lesson
                {
                    PathNodeId = pathNodeId,
                    Position = seed.Position,
                    XpReward = seed.XpReward,
                    Active = seed.Active
                });

                created++;
            }

            if (created > 0)
            {
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("{Count} lição(ões) inserida(s).", created);
            }

            return await LoadLessonsAsync(cancellationToken);
        }

        private async Task<Dictionary<(int PathNodeId, int Position), int>> LoadLessonsAsync(CancellationToken cancellationToken)
        {
            var lessons = await _context.Lessons
                .Select(l => new { l.Id, l.PathNodeId, l.Position })
                .ToListAsync(cancellationToken);

            return lessons.ToDictionary(l => (l.PathNodeId, l.Position), l => l.Id);
        }

        private async Task SeedLessonWordsAsync(
            string seedPath,
            Dictionary<string, int> languages,
            Dictionary<(int LanguageId, string Name), int> courses,
            Dictionary<(int CourseId, string Title), int> units,
            Dictionary<(int UnitId, int Position), int> nodes,
            Dictionary<(int PathNodeId, int Position), int> lessons,
            Dictionary<(int LanguageId, string Text), int> words,
            CancellationToken cancellationToken)
        {
            var seeds = await ReadAsync<LessonWordSeed>(seedPath, "lesson-words.json", cancellationToken);

            var existing = (await _context.LessonWords
                .Select(lw => new { lw.LessonId, lw.WordId })
                .ToListAsync(cancellationToken))
                .Select(lw => (lw.LessonId, lw.WordId))
                .ToHashSet();

            var created = 0;

            foreach (var seed in seeds)
            {
                if (!languages.TryGetValue(seed.LanguageCode.ToLowerInvariant(), out var languageId))
                {
                    _logger.LogWarning("Idioma {LanguageCode} não encontrado para o vínculo lição/palavra.", seed.LanguageCode);
                    continue;
                }

                if (!TryResolveLesson(
                        languages,
                        courses,
                        units,
                        nodes,
                        lessons,
                        seed.LanguageCode,
                        seed.CourseName,
                        seed.UnitTitle,
                        seed.NodePosition,
                        seed.LessonPosition,
                        out var lessonId))
                {
                    _logger.LogWarning("Lição da unidade {Unit} não encontrada para o vínculo lição/palavra.", seed.UnitTitle);
                    continue;
                }

                if (!words.TryGetValue((languageId, seed.WordText.ToLowerInvariant()), out var wordId))
                {
                    _logger.LogWarning("Palavra {Word} não encontrada para a unidade {Unit}.", seed.WordText, seed.UnitTitle);
                    continue;
                }

                if (!existing.Add((lessonId, wordId)))
                {
                    continue;
                }

                _context.LessonWords.Add(new LessonWord
                {
                    LessonId = lessonId,
                    WordId = wordId,
                    Position = seed.Position
                });

                created++;
            }

            if (created > 0)
            {
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("{Count} vínculo(s) lição/palavra inserido(s).", created);
            }
        }

        private async Task<Dictionary<string, int>> SeedUsersAsync(
            string seedPath,
            Dictionary<string, int> languages,
            CancellationToken cancellationToken)
        {
            var seeds = await ReadAsync<UserSeed>(seedPath, "users.json", cancellationToken);

            var existing = await LoadUsersAsync(cancellationToken);
            var created = 0;

            foreach (var seed in seeds)
            {
                if (existing.ContainsKey(seed.Email.ToLowerInvariant()))
                {
                    continue;
                }

                if (!languages.TryGetValue(seed.NativeLanguageCode.ToLowerInvariant(), out var nativeLanguageId))
                {
                    _logger.LogWarning("Idioma nativo {LanguageCode} não encontrado para o usuário {Email}.", seed.NativeLanguageCode, seed.Email);
                    continue;
                }

                _context.Users.Add(new User
                {
                    Name = seed.Name,
                    Email = seed.Email,
                    NativeLanguageId = nativeLanguageId,
                    CreatedAt = DateTime.UtcNow,
                    Active = seed.Active
                });

                created++;
            }

            if (created > 0)
            {
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("{Count} usuário(s) inserido(s).", created);
            }

            var users = await LoadUsersAsync(cancellationToken);

            await SeedLanguageProgressesAsync(seeds, languages, users, cancellationToken);

            return users;
        }

        private async Task<Dictionary<string, int>> LoadUsersAsync(CancellationToken cancellationToken)
        {
            var users = await _context.Users
                .Select(u => new { u.Id, u.Email })
                .ToListAsync(cancellationToken);

            return users.ToDictionary(u => u.Email.ToLowerInvariant(), u => u.Id);
        }

        private async Task SeedLanguageProgressesAsync(
            List<UserSeed> seeds,
            Dictionary<string, int> languages,
            Dictionary<string, int> users,
            CancellationToken cancellationToken)
        {
            var existing = (await _context.LanguageProgresses
                .Select(lp => new { lp.UserId, lp.LanguageId })
                .ToListAsync(cancellationToken))
                .Select(lp => (lp.UserId, lp.LanguageId))
                .ToHashSet();

            var created = 0;

            foreach (var seed in seeds)
            {
                if (string.IsNullOrWhiteSpace(seed.LearningLanguageCode))
                {
                    continue;
                }

                if (!users.TryGetValue(seed.Email.ToLowerInvariant(), out var userId))
                {
                    continue;
                }

                if (!languages.TryGetValue(seed.LearningLanguageCode.ToLowerInvariant(), out var languageId))
                {
                    _logger.LogWarning("Idioma {LanguageCode} não encontrado para o progresso do usuário {Email}.", seed.LearningLanguageCode, seed.Email);
                    continue;
                }

                if (!existing.Add((userId, languageId)))
                {
                    continue;
                }

                // O vínculo com o curso é criado zerado: o usuário começa no nível 1,
                // sem XP e sem ofensiva, com toda a trilha bloqueada.
                _context.LanguageProgresses.Add(new LanguageProgress
                {
                    UserId = userId,
                    LanguageId = languageId,
                    Level = 1,
                    TotalXp = 0,
                    CurrentStreakDays = 0,
                    TotalLearnedWords = 0,
                    TotalCompletedLessons = 0,
                    IsActiveCourse = true,
                    CreatedAt = DateTime.UtcNow
                });

                created++;
            }

            if (created > 0)
            {
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("{Count} progresso(s) de idioma inserido(s).", created);
            }
        }

        private static bool TryResolveCourse(
            Dictionary<string, int> languages,
            Dictionary<(int LanguageId, string Name), int> courses,
            string languageCode,
            string courseName,
            out int courseId)
        {
            courseId = 0;

            return languages.TryGetValue(languageCode.ToLowerInvariant(), out var languageId)
                && courses.TryGetValue((languageId, courseName.ToLowerInvariant()), out courseId);
        }

        private static bool TryResolveUnit(
            Dictionary<string, int> languages,
            Dictionary<(int LanguageId, string Name), int> courses,
            Dictionary<(int CourseId, string Title), int> units,
            string languageCode,
            string courseName,
            string unitTitle,
            out int unitId)
        {
            unitId = 0;

            return TryResolveCourse(languages, courses, languageCode, courseName, out var courseId)
                && units.TryGetValue((courseId, unitTitle.ToLowerInvariant()), out unitId);
        }

        private static bool TryResolveNode(
            Dictionary<string, int> languages,
            Dictionary<(int LanguageId, string Name), int> courses,
            Dictionary<(int CourseId, string Title), int> units,
            Dictionary<(int UnitId, int Position), int> nodes,
            string languageCode,
            string courseName,
            string unitTitle,
            int nodePosition,
            out int pathNodeId)
        {
            pathNodeId = 0;

            return TryResolveUnit(languages, courses, units, languageCode, courseName, unitTitle, out var unitId)
                && nodes.TryGetValue((unitId, nodePosition), out pathNodeId);
        }

        private static bool TryResolveLesson(
            Dictionary<string, int> languages,
            Dictionary<(int LanguageId, string Name), int> courses,
            Dictionary<(int CourseId, string Title), int> units,
            Dictionary<(int UnitId, int Position), int> nodes,
            Dictionary<(int PathNodeId, int Position), int> lessons,
            string languageCode,
            string courseName,
            string unitTitle,
            int nodePosition,
            int lessonPosition,
            out int lessonId)
        {
            lessonId = 0;

            return TryResolveNode(languages, courses, units, nodes, languageCode, courseName, unitTitle, nodePosition, out var pathNodeId)
                && lessons.TryGetValue((pathNodeId, lessonPosition), out lessonId);
        }
    }
}
