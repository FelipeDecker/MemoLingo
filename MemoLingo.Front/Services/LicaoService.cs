using MemoLingo.Front.Models;

namespace MemoLingo.Front.Services
{
    /// <summary>
    /// Implementação MOCK de <see cref="ILicaoService"/>.
    /// Enquanto não existe integração com a API/banco de dados real, esta classe simula
    /// a resposta que viria do backend (com um pequeno delay) para que a tela "Learn"
    /// possa ser construída e testada de forma independente.
    /// </summary>
    public class LicaoService : ILicaoService
    {
        public async Task<List<Unidade>> ObterUnidadesAsync()
        {
            // Simula a latência de uma chamada real a uma API.
            await Task.Delay(300);

            return ObterUnidadesMock();
        }

        /// <summary>
        /// Dados fixos (mock) representando as unidades e lições que, futuramente,
        /// virão de uma API/banco de dados. Cada unidade agrupa suas lições através do UnidadeId.
        /// </summary>
        private static List<Unidade> ObterUnidadesMock()
        {
            var unidades = new List<Unidade>
            {
                new Unidade
                {
                    Id = 1,
                    Nome = "Unidade 1",
                    Descricao = "Frases básicas do dia a dia",
                    CorPrimaria = "#58cc02"
                },
                new Unidade
                {
                    Id = 2,
                    Nome = "Unidade 2",
                    Descricao = "Gratidão: agradeça pela ajuda",
                    CorPrimaria = "#1cb0f6"
                },
                new Unidade
                {
                    Id = 3,
                    Nome = "Unidade 3",
                    Descricao = "Comidas e bebidas",
                    CorPrimaria = "#ce82ff"
                }
            };

            // Definição de quantas lições cada unidade tem e como elas se dividem por tipo.
            // Isso simula o "foreach de quantos publicar" pedido: cada item da lista abaixo
            // vira uma bolinha na trilha, agrupada pela UnidadeId.
            var licoesPorUnidade = new Dictionary<int, List<(TipoLicao Tipo, StatusLicao Status)>>
            {
                [1] = new()
                {
                    (TipoLicao.Licao, StatusLicao.Concluida),
                    (TipoLicao.Licao, StatusLicao.Concluida),
                    (TipoLicao.Historia, StatusLicao.Concluida),
                    (TipoLicao.Licao, StatusLicao.Concluida),
                    (TipoLicao.Bau, StatusLicao.Concluida),
                    (TipoLicao.Exame, StatusLicao.Concluida)
                },
                [2] = new()
                {
                    (TipoLicao.Licao, StatusLicao.Concluida),
                    (TipoLicao.Historia, StatusLicao.Concluida),
                    (TipoLicao.Licao, StatusLicao.Concluida),
                    (TipoLicao.Bau, StatusLicao.Disponivel),
                    (TipoLicao.Licao, StatusLicao.Disponivel),
                    (TipoLicao.Licao, StatusLicao.Bloqueada),
                    (TipoLicao.Exame, StatusLicao.Bloqueada)
                },
                [3] = new()
                {
                    (TipoLicao.Licao, StatusLicao.Bloqueada),
                    (TipoLicao.Licao, StatusLicao.Bloqueada),
                    (TipoLicao.Historia, StatusLicao.Bloqueada),
                    (TipoLicao.Bau, StatusLicao.Bloqueada),
                    (TipoLicao.Exame, StatusLicao.Bloqueada)
                }
            };

            var idLicao = 1;

            // Foreach que "publica" cada lição mockada dentro da unidade correspondente,
            // preenchendo o UnidadeId e a ordem de exibição na trilha.
            foreach (var unidade in unidades)
            {
                var ordem = 1;

                foreach (var (tipo, status) in licoesPorUnidade[unidade.Id])
                {
                    unidade.Licoes.Add(new Licao
                    {
                        Id = idLicao++,
                        UnidadeId = unidade.Id,
                        Titulo = $"{unidade.Nome} - Lição {ordem}",
                        Tipo = tipo,
                        Status = status,
                        Ordem = ordem
                    });

                    ordem++;
                }
            }

            return unidades;
        }
    }
}
