using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using MemoLingo.Application.Models;
using MemoLingo.Domain.Entities;
using MemoLingo.Domain.Enums;

namespace MemoLingo.Application.Services
{
    // Regras compartilhadas pelas atividades baseadas em frases (lições e teste de salto de seção):
    // montagem dos exercícios e correção tolerante das respostas.
    internal static class SentenceExerciseHelper
    {
        public const string BlankToken = "{{blank}}";

        private const int MaxBlankOptions = 4;
        private const int MinBlankOptions = 2;
        private const int MaxAnswerLength = 500;
        private const char AlternativesSeparator = '|';

        // Contrações do inglês expandidas antes da comparação ("I'm" = "I am").
        private static readonly (Regex Pattern, string Replacement)[] Contractions =
        {
            (new Regex(@"\bcan't\b", RegexOptions.Compiled), "cannot"),
            (new Regex(@"\bcan not\b", RegexOptions.Compiled), "cannot"),
            (new Regex(@"\bwon't\b", RegexOptions.Compiled), "will not"),
            (new Regex(@"\b(\w+)n't\b", RegexOptions.Compiled), "$1 not"),
            (new Regex(@"\bi'm\b", RegexOptions.Compiled), "i am"),
            (new Regex(@"\b(\w+)'re\b", RegexOptions.Compiled), "$1 are"),
            (new Regex(@"\b(he|she|it|that|what|where|who|there|name|here)'s\b", RegexOptions.Compiled), "$1 is"),
            (new Regex(@"\b(\w+)'ll\b", RegexOptions.Compiled), "$1 will"),
            (new Regex(@"\b(\w+)'ve\b", RegexOptions.Compiled), "$1 have"),
            (new Regex(@"\b(\w+)'d\b", RegexOptions.Compiled), "$1 would")
        };

        // Contrações escritas sem apóstrofo ("dont", "im"), aceitas apenas para quem ativou essa opção
        // no perfil (Premium). Ficam de fora as formas que também são palavras comuns ("its", "were",
        // "well", "ill", "id", "shell", "hell", "wed", "lets").
        private static readonly (Regex Pattern, string Replacement)[] ContractionsWithoutApostrophe =
        {
            (new Regex(@"\bcant\b", RegexOptions.Compiled), "cannot"),
            (new Regex(@"\bwont\b", RegexOptions.Compiled), "will not"),
            (new Regex(@"\b(do|does|did|is|are|was|were|has|have|had|would|could|should|must|need)nt\b", RegexOptions.Compiled), "$1 not"),
            (new Regex(@"\bim\b", RegexOptions.Compiled), "i am"),
            (new Regex(@"\b(you|they)re\b", RegexOptions.Compiled), "$1 are"),
            (new Regex(@"\b(he|she|that|what|where|who|there|here)s\b", RegexOptions.Compiled), "$1 is"),
            (new Regex(@"\b(you|they)ll\b", RegexOptions.Compiled), "$1 will"),
            (new Regex(@"\b(i|you|we|they)ve\b", RegexOptions.Compiled), "$1 have"),
            (new Regex(@"\b(you|they)d\b", RegexOptions.Compiled), "$1 would")
        };

        public static LessonExerciseModel CreateTranslation(Sentence sentence, ExerciseType exerciseType)
        {
            return new LessonExerciseModel
            {
                SentenceId = sentence.Id,
                ExerciseType = exerciseType,
                Prompt = exerciseType == ExerciseType.TranslationToNative ? sentence.Text : sentence.Translation
            };
        }

        // Cria um exercício de lacuna. A palavra escondida é escolhida entre as palavras do vocabulário
        // presentes na frase, pela maior prioridade (empates são sorteados). As opções erradas vêm do
        // mesmo vocabulário, preferindo a mesma classe gramatical.
        public static LessonExerciseModel CreateFillInTheBlank(Sentence sentence, IReadOnlyCollection<Word> vocabulary, Func<Word, double> priority)
        {
            var target = vocabulary
                .Select(w => new { Word = w, Match = FindTerm(sentence.Text, w.Text) })
                .Where(c => c.Match is not null)
                .OrderByDescending(c => priority(c.Word))
                .ThenBy(_ => Random.Shared.Next())
                .FirstOrDefault();

            if (target is null)
            {
                return null;
            }

            var distractors = vocabulary
                .Where(w => w.Id != target.Word.Id
                    && !string.Equals(w.Text, target.Word.Text, StringComparison.OrdinalIgnoreCase)
                    && FindTerm(sentence.Text, w.Text) is null)
                .OrderByDescending(w => w.PartOfSpeech == target.Word.PartOfSpeech)
                .ThenBy(_ => Random.Shared.Next())
                .Select(w => w.Text)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(MaxBlankOptions - 1)
                .ToList();

            if (distractors.Count + 1 < MinBlankOptions)
            {
                return null;
            }

            var options = Shuffle(distractors.Append(target.Word.Text).ToList());
            var match = target.Match;

            return new LessonExerciseModel
            {
                SentenceId = sentence.Id,
                ExerciseType = ExerciseType.FillInTheBlank,
                Prompt = sentence.Text[..match.Index] + BlankToken + sentence.Text[(match.Index + match.Length)..],
                Hint = sentence.Translation,
                BlankWordId = target.Word.Id,
                Options = options
            };
        }

        // Resolve a resposta esperada e as respostas aceitas de um exercício. Para lacunas, a palavra
        // informada precisa aparecer na frase.
        public static (string CorrectAnswer, List<string> AcceptedAnswers) GetExpectedAnswers(Sentence sentence, ExerciseType exerciseType, Word blankWord)
        {
            switch (exerciseType)
            {
                case ExerciseType.TranslationToNative:
                    return (sentence.Translation, SplitAlternatives(sentence.AlternativeTranslations).Prepend(sentence.Translation).ToList());

                case ExerciseType.TranslationToTarget:
                    return (sentence.Text, SplitAlternatives(sentence.AlternativeTexts).Prepend(sentence.Text).ToList());

                case ExerciseType.FillInTheBlank:
                    var match = blankWord is null ? null : FindTerm(sentence.Text, blankWord.Text);

                    if (match is null)
                    {
                        throw new ArgumentException("A palavra da lacuna não pertence à frase.", nameof(blankWord));
                    }

                    return (match.Value, new List<string> { match.Value, blankWord.Text });

                default:
                    throw new ArgumentException("Tipo de exercício não suportado nesta atividade.", nameof(exerciseType));
            }
        }

        // A tolerância a contrações sem apóstrofo é uma preferência exclusiva do plano Premium.
        public static bool AcceptsMissingApostrophes(User user)
        {
            return user.Plan == SubscriptionPlan.Premium && user.AcceptMissingApostrophes;
        }

        public static bool IsCorrect(string answer, IEnumerable<string> acceptedAnswers, bool allowMissingApostrophes)
        {
            var normalizedAnswer = Normalize(answer, allowMissingApostrophes);

            return normalizedAnswer.Length > 0
                && acceptedAnswers.Select(a => Normalize(a, allowMissingApostrophes)).Contains(normalizedAnswer);
        }

        // Procura a palavra/expressão como termo inteiro na frase (ex.: "man" não casa com "woman").
        public static Match FindTerm(string text, string term)
        {
            if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(term))
            {
                return null;
            }

            var pattern = @"(?<![\p{L}'])" + Regex.Escape(term.Trim()).Replace(@"\ ", @"\s+") + @"(?![\p{L}'])";
            var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

            return match.Success ? match : null;
        }

        public static string Truncate(string value)
        {
            return value is { Length: > MaxAnswerLength } ? value[..MaxAnswerLength] : value;
        }

        public static List<T> Shuffle<T>(List<T> items)
        {
            for (var i = items.Count - 1; i > 0; i--)
            {
                var j = Random.Shared.Next(i + 1);
                (items[i], items[j]) = (items[j], items[i]);
            }

            return items;
        }

        private static IEnumerable<string> SplitAlternatives(string alternatives)
        {
            return string.IsNullOrWhiteSpace(alternatives)
                ? Enumerable.Empty<string>()
                : alternatives.Split(AlternativesSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        }

        // Normaliza a resposta para uma comparação tolerante: ignora maiúsculas, acentos,
        // pontuação e espaços extras, e expande as contrações do inglês.
        private static string Normalize(string value, bool allowMissingApostrophes)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var text = value
                .Replace('\u2019', '\'')
                .Replace('\u2018', '\'')
                .ToLowerInvariant();

            foreach (var (pattern, replacement) in Contractions)
            {
                text = pattern.Replace(text, replacement);
            }

            if (allowMissingApostrophes)
            {
                foreach (var (pattern, replacement) in ContractionsWithoutApostrophe)
                {
                    text = pattern.Replace(text, replacement);
                }
            }

            var builder = new StringBuilder(text.Length);

            foreach (var character in text.Normalize(NormalizationForm.FormD))
            {
                var category = CharUnicodeInfo.GetUnicodeCategory(character);

                if (category == UnicodeCategory.NonSpacingMark)
                {
                    continue;
                }

                builder.Append(char.IsLetterOrDigit(character) ? character : ' ');
            }

            return Regex.Replace(builder.ToString(), @"\s+", " ").Trim();
        }
    }
}
