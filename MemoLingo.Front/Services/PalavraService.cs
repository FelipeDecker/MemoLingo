using MemoLingo.Front.Models;

namespace MemoLingo.Front.Services
{
    /// <summary>
    /// Implementação MOCK de <see cref="IPalavraService"/>. Enquanto não existe
    /// integração com a API/banco de dados real, esta classe simula o histórico
    /// de erros e acertos do usuário para viabilizar a construção da tela de Prática.
    /// </summary>
    public class PalavraService : IPalavraService
    {
        private readonly List<Palavra> palavras = ObterPalavrasMock();

        public async Task<List<Palavra>> ObterPalavrasParaPraticaAsync()
        {
            // Simula a latência de uma chamada real a uma API.
            await Task.Delay(300);

            // Diferencial do app: prioriza as palavras com mais erros (menor taxa de acerto primeiro).
            return palavras
                .OrderByDescending(p => p.QuantidadeErros)
                .ThenBy(p => p.TaxaAcerto)
                .ToList();
        }

        public Task RegistrarResultadoAsync(int palavraId, bool acertou)
        {
            var palavra = palavras.FirstOrDefault(p => p.Id == palavraId);
            if (palavra != null)
            {
                if (acertou)
                {
                    palavra.QuantidadeAcertos++;
                }
                else
                {
                    palavra.QuantidadeErros++;
                }
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Dados fixos (mock) simulando o histórico de erros/acertos do usuário
        /// para algumas palavras já estudadas na trilha.
        /// </summary>
        private static List<Palavra> ObterPalavrasMock()
        {
            return new List<Palavra>
            {
                new Palavra { Id = 1, Texto = "obrigado", Traducao = "thank you", QuantidadeErros = 7, QuantidadeAcertos = 2 },
                new Palavra { Id = 2, Texto = "convite", Traducao = "invitation", QuantidadeErros = 5, QuantidadeAcertos = 1 },
                new Palavra { Id = 3, Texto = "ajuda", Traducao = "help", QuantidadeErros = 4, QuantidadeAcertos = 3 },
                new Palavra { Id = 4, Texto = "comida", Traducao = "food", QuantidadeErros = 3, QuantidadeAcertos = 5 },
                new Palavra { Id = 5, Texto = "bebida", Traducao = "drink", QuantidadeErros = 2, QuantidadeAcertos = 4 },
                new Palavra { Id = 6, Texto = "gentileza", Traducao = "kindness", QuantidadeErros = 2, QuantidadeAcertos = 1 },
                new Palavra { Id = 7, Texto = "família", Traducao = "family", QuantidadeErros = 1, QuantidadeAcertos = 6 },
                new Palavra { Id = 8, Texto = "amigo", Traducao = "friend", QuantidadeErros = 1, QuantidadeAcertos = 8 },
                new Palavra { Id = 9, Texto = "casa", Traducao = "house", QuantidadeErros = 0, QuantidadeAcertos = 9 },
                new Palavra { Id = 10, Texto = "escola", Traducao = "school", QuantidadeErros = 0, QuantidadeAcertos = 7 },
            };
        }
    }
}
