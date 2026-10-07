using System.Text.RegularExpressions;

namespace MemoLingo.Infrastructure.Data.Seeding
{
    // Encontra em uma frase as palavras/expressões cadastradas, priorizando as mais longas
    // ("good morning" antes de "good"), aceitando plurais/3ª pessoa simples ("mothers", "eats")
    // e verbos do currículo no infinitivo ("to eat") conjugados na frase.
    public class SentenceWordMatcher
    {
        private const int MinLengthForInflection = 3;
        private const string NounSuffix = "(?:s|es|'s)?";
        private const string VerbSuffix = "(?:s|es|d|ed|ing)?";

        private readonly List<(int WordId, Regex Pattern)> _patterns;

        public SentenceWordMatcher(IEnumerable<(int WordId, string Text)> words)
        {
            _patterns = words
                .Where(w => !string.IsNullOrWhiteSpace(w.Text))
                .Select(w => (w.WordId, Text: w.Text.Trim()))
                .OrderByDescending(w => w.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length)
                .ThenByDescending(w => w.Text.Length)
                .Select(w => (w.WordId, BuildPattern(w.Text)))
                .ToList();
        }

        public List<int> Match(string sentence)
        {
            var found = new List<(int WordId, int Index)>();

            if (string.IsNullOrWhiteSpace(sentence))
            {
                return new List<int>();
            }

            var text = sentence.Replace('\u2019', '\'');

            foreach (var (wordId, pattern) in _patterns)
            {
                var match = pattern.Match(text);

                if (!match.Success)
                {
                    continue;
                }

                found.Add((wordId, match.Index));

                // Mascara o trecho para que partes de uma expressão não sejam contadas de novo.
                text = text[..match.Index] + new string(' ', match.Length) + text[(match.Index + match.Length)..];
            }

            return found
                .OrderBy(f => f.Index)
                .Select(f => f.WordId)
                .Distinct()
                .ToList();
        }

        private static Regex BuildPattern(string term)
        {
            var tokens = term.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var alternatives = new List<string> { BuildAlternative(tokens, NounSuffix) };

            // Verbos ficam no currículo no infinitivo ("to eat"), mas aparecem nas frases conjugados ("eats").
            if (tokens.Length > 1 && string.Equals(tokens[0], "to", StringComparison.OrdinalIgnoreCase))
            {
                alternatives.Add(BuildAlternative(tokens[1..], VerbSuffix));
            }

            return new Regex(
                @"(?<![\p{L}'])(?:" + string.Join("|", alternatives) + @")(?![\p{L}'])",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        }

        private static string BuildAlternative(string[] tokens, string suffix)
        {
            var body = string.Join(@"\s+", tokens.Select(Regex.Escape));

            // Palavras muito curtas não recebem sufixo para evitar falsos positivos ("i" + "s" = "is").
            return tokens[^1].Length >= MinLengthForInflection ? body + suffix : body;
        }
    }
}
