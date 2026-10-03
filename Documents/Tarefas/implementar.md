Atue como um arquiteto de software e engenheiro backend sênior.

Preciso implementar uma nova funcionalidade no meu sistema de aprendizado de idiomas (estilo Duolingo), focado no aprendizado por repetição de erros. A funcionalidade será uma nova seção na aba de prática chamada "Nuance / Contextual Discrimination" (discriminação de quase-sinônimos).

### Objetivo da Feature:

Diferenciar termos com traduções similares que dependem estritamente do contexto (exemplos: "guilt" vs. "blame" vs. "fault"; "deal with" vs. "handle"; "plain" vs. "simple").
O exercício consiste em apresentar uma frase contextualizada e detalhada contendo uma lacuna `{{blank}}`, onde o usuário deve preencher/selecionar a palavra semanticamente correta daquele grupo, recebendo uma explicação comparativa do porquê os outros termos não se aplicam ali.

### O que você deve projetar e implementar:

1. **Schema do Banco de Dados / Entidades (ORM/Migrations):**
   - Entidade para o grupo de sinônimos/nuances (ex: `SynonymGroup` / `NuanceCluster`).
   - Tabela intermediária associando as palavras ao grupo, contendo a explicação da nuance de cada palavra individual (ex: `SynonymGroupItem` com campo `nuance_explanation`).
   - Entidade do exercício (ex: `NuanceExercise`) contendo:
     - Frase com lacuna (`sentence_context`).
     - Referência à palavra correta (`target_word_id`).
     - Explicação da resposta correta vs. as outras do grupo (`explanation`).
   - Entidade de rastreamento do usuário (ex: `UserNuanceProgress` com histórico de tentativas, acertos e priorização baseada em repetição espaçada).

2. **Regras de Negócio e Serviços (Service Layer):**
   - Método para carregar uma sessão de prática de nuances prioritária (priorizando grupos onde o usuário mais cometeu erros recentemente).
   - Validador da resposta: recebe o `exercise_id` e o `submitted_word` (ou `word_id`), valida o acerto e retorna a resolução completa: se acertou/errou, a palavra correta, a explicação do contexto e o resumo das nuances de todas as palavras do grupo.
   - Atualização do score de proficiência do usuário naquele grupo.

3. **Integração com o Fluxo de Erros Existente:**
   - Lógica ou hook para identificar quando um erro cometido em um exercício comum pertence a um `SynonymGroup`, marcando esse grupo para revisão prioritária.

Forneça:

- O código do schema/entidades.
- A camada de serviço com a lógica de validação e geração da sessão.
- Um payload JSON de exemplo para a requisição de validação e para a resposta do exercício.
