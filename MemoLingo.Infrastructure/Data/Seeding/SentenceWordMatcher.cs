using System.Text.RegularExpressions;
using MemoLingo.Domain.Enums;

namespace MemoLingo.Infrastructure.Data.Seeding
{
    // Encontra em uma frase as palavras/expressões cadastradas, priorizando os trechos mais longos
    // ("good morning" antes de "good", "have to" antes de "have"), aceitando plurais/3ª pessoa
    // simples ("mothers", "eats") e verbos do currículo no infinitivo ("to eat") conjugados na frase.
    // Em trechos de mesmo tamanho, a palavra escrita exatamente igual vence a forma flexionada
    // ("thanks" é a palavra "thanks", não o verbo "to thank" conjugado). Homógrafos que disputam o
    // mesmo trecho ("watch" e "to watch") são todos vinculados, desde que sejam do menor nível CEFR
    // entre eles ("book" A1 vence "to book" A2 em "I read a book").
    public class SentenceWordMatcher
    {
        private const int MinLengthForInflection = 3;
        private const string NounSuffix = "(?:s|es|'s)?";
        private const string VerbSuffix = "(?:s|es|d|ed|ing)?";

        private readonly List<(int WordId, string Text, CefrLevel Level, Regex Pattern)> _patterns;

        public SentenceWordMatcher(IEnumerable<(int WordId, string Text, CefrLevel Level)> words)
        {
            _patterns = words
                .Where(w => !string.IsNullOrWhiteSpace(w.Text))
                .Select(w => (w.WordId, Text: w.Text.Trim(), w.Level))
                .SelectMany(w => BuildPatterns(w.Text).Select(pattern => (w.WordId, w.Text, w.Level, pattern)))
                .ToList();
        }

        public List<int> Match(string sentence)
        {
            if (string.IsNullOrWhiteSpace(sentence))
            {
                return new List<int>();
            }

            var text = sentence.Replace('\u2019', '\'');

            var candidates = _patterns
                .SelectMany(p => p.Pattern.Matches(text).Select(m => new
                {
                    p.WordId,
                    p.Level,
                    m.Index,
                    m.Length,
                    IsExact = IsExactForm(m.Value, p.Text)
                }))
                .OrderByDescending(c => c.Length)
                .ThenByDescending(c => c.IsExact)
                .ThenBy(c => c.Level)
                .ThenBy(c => c.Index)
                .ToList();

            var taken = new bool[text.Length];
            var claimed = new HashSet<(int Index, int Length, bool IsExact, CefrLevel Level)>();
            var found = new List<(int WordId, int Index)>();

            foreach (var candidate in candidates)
            {
                var span = (candidate.Index, candidate.Length, candidate.IsExact, candidate.Level);

                // Cada trecho da frase pertence a uma única palavra/expressão, exceto quando homógrafos
                // do mesmo nível disputam o mesmo trecho: nesse caso todos são vinculados.
                if (!claimed.Contains(span))
                {
                    if (Enumerable.Range(candidate.Index, candidate.Length).Any(i => taken[i]))
                    {
                        continue;
                    }

                    for (var i = candidate.Index; i < candidate.Index + candidate.Length; i++)
                    {
                        taken[i] = true;
                    }

                    claimed.Add(span);
                }

                found.Add((candidate.WordId, candidate.Index));
            }

            return found
                .OrderBy(f => f.Index)
                .Select(f => f.WordId)
                .Distinct()
                .ToList();
        }

        private static bool IsExactForm(string matched, string term)
        {
            var normalized = Regex.Replace(matched, @"\s+", " ");

            return string.Equals(normalized, term, StringComparison.OrdinalIgnoreCase)
                || (term.StartsWith("to ", StringComparison.OrdinalIgnoreCase)
                    && string.Equals(normalized, term[3..], StringComparison.OrdinalIgnoreCase));
        }

        private static List<Regex> BuildPatterns(string term)
        {
            var tokens = term.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var alternatives = new List<string> { BuildAlternative(tokens, NounSuffix) };

            // Verbos ficam no currículo no infinitivo ("to eat"), mas aparecem nas frases conjugados ("eats").
            if (tokens.Length > 1 && string.Equals(tokens[0], "to", StringComparison.OrdinalIgnoreCase))
            {
                alternatives.Add(BuildAlternative(tokens[1..], VerbSuffix));
            }

            // Cada forma vira um padrão separado para que "to go" dentro de "have to go" ainda
            // encontre o "go" mesmo quando o "to" já pertence a outra expressão.
            return alternatives
                .Select(alternative => new Regex(
                    @"(?<![\p{L}'])(?:" + alternative + @")(?![\p{L}'])",
                    RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
                .ToList();
        }

        private static string BuildAlternative(string[] tokens, string suffix)
        {
            var body = string.Join(@"\s+", tokens.Select(Regex.Escape));

            // Palavras muito curtas não recebem sufixo para evitar falsos positivos ("i" + "s" = "is").
            return tokens[^1].Length >= MinLengthForInflection ? body + suffix : body;
        }
    }
}
