# Entregas concluídas — MemoLingo

Registro histórico de tudo que já foi implementado no projeto, agrupado por área.
Itens já concluídos saem do [TODO.md](../TODO.md) (que passa a listar só o que falta)
e passam a ser documentados aqui.

> Fases originais **1 (Modelagem de domínio)** e **2 (Banco de dados)** foram
> concluídas por completo e removidas do TODO; as fases seguintes foram
> renumeradas.

---

## 1. Infraestrutura e estrutura do projeto

| Entrega | Onde está |
|---|---|
| Solução .NET 10 com todos os projetos | `MemoLingo.slnx` |
| Frontend Blazor WebAssembly | `MemoLingo.Front/` |
| Backend ASP.NET Core Web API | `MemoLingo.Api/` |
| Domínio puro (entidades, enums, contratos) | `MemoLingo.Domain/` |
| Camada de aplicação (serviços e models) | `MemoLingo.Application/` |
| Camada de infraestrutura (EF Core) | `MemoLingo.Infrastructure/` |
| Cliente HTTP tipado gerado por NSwag | `MemoLingo.Api.Client/` |
| Dockerfile da API | `MemoLingo.Api/Dockerfile` |
| Docker Compose (API + PostgreSQL 17 com healthcheck e volume) | `docker-compose.yml` |
| Injeção de dependência da infraestrutura (`AddInfrastructure`) | `MemoLingo.Infrastructure/DependencyInjection.cs` |
| Repositório genérico (`IGenericRepository<T>` / `GenericRepository<T>`) | `MemoLingo.Domain/Repositories/`, `MemoLingo.Infrastructure/Repositories/` |
| CORS liberado para o front + OpenAPI/Swagger em desenvolvimento | `MemoLingo.Api/Program.cs` |

## 2. Domínio

Todas as entidades vivem em `MemoLingo.Domain/Entities` e os enums em
`MemoLingo.Domain/Enums`.

### Entidades

| Entidade | Responsabilidade |
|---|---|
| `Language` | Idioma (nativo ou alvo), com `Code` único |
| `Word` | Palavra: texto, tradução, idioma, nível CEFR e classe gramatical |
| `Sentence` | Frase: texto, tradução, idioma e nível CEFR |
| `SentenceWord` | Vínculo N:N entre frase e palavra |
| `Course` | Trilha/curso (ex.: "Básico", "Viagem") com posição |
| `Lesson` | Lição pertencente a uma trilha, com posição |
| `LessonWord` | Vínculo N:N entre lição e palavra |
| `User` | Usuário (nome, e-mail único, hash de senha, idioma nativo, ativo) |
| `LanguageProgress` | Progresso do usuário por idioma: nível, XP, streak, curso ativo, totais |
| `WordPerformance` | Desempenho usuário × palavra: acertos, erros, força, última e próxima revisão |
| `ExerciseAttempt` | Log de cada resposta: certo/errado, tempo de resposta, timestamp |
| `StudySession` | Agrupa as tentativas respondidas em uma sessão de estudo |

### Enums

`CefrLevel`, `ExerciseType` (múltipla escolha, tradução livre, completar frase,
ouvir e escrever, falar), `PartOfSpeech`, `ProgressStatus`.

### Documentação

Diagrama ER do domínio em [`domain-model.md`](./domain-model.md).

## 3. Banco de dados (EF Core + PostgreSQL)

- **Banco definitivo escolhido:** PostgreSQL (dev via Docker Compose e produção).
- **`AppDbContext`** com todo o mapeamento fluente em
  `MemoLingo.Infrastructure/Data/AppDbContext.cs`.
- **Migration inicial** `20260903173033_InitialCreate` em
  `MemoLingo.Infrastructure/Data/Migrations/`.
- **Migrations aplicadas automaticamente** no start da API (`db.Database.Migrate()`
  em `MemoLingo.Api/Program.cs`), com bypass no ambiente `NSwagGenerator`.

### Índices criados

Definidos em `AppDbContext.OnModelCreating` e materializados na migration inicial.

| Tabela | Índice | Objetivo |
|---|---|---|
| `Users` | `Email` (único) | Login / unicidade |
| `Languages` | `Code` (único) | Busca por código do idioma |
| `LanguageProgresses` | `UserId, LanguageId` (único) | Um progresso por idioma |
| `Words` | `LanguageId, Text` (único) | Unicidade da palavra no idioma |
| `Words` | `LanguageId, CefrLevel` | Seleção de conteúdo por nível |
| `Sentences` | `LanguageId, CefrLevel` | Seleção de frases por nível |
| `SentenceWords` | `SentenceId, WordId` (único) | Vínculo frase × palavra |
| `Courses` | `LanguageId, Position` | Ordenação da trilha |
| `Lessons` | `CourseId, Position` | Ordenação das lições |
| `LessonWords` | `LessonId, WordId` (único) | Vínculo lição × palavra |
| `StudySessions` | `UserId, StartedAt` | Histórico/streak do usuário |
| `ExerciseAttempts` | `WordId, AnsweredAt` | Taxa de erro recente por palavra |
| `WordPerformances` | `UserId, WordId` (único) | Um registro por usuário × palavra |
| `WordPerformances` | `UserId, NextReview` | Fila de revisão (SRS) |
| `WordPerformances` | `UserId, LastReview` | Priorização por recência |

> Os três últimos são exatamente os **índices de priorização (usuário + palavra +
> última revisão)**: `IX_WordPerformances_UserId_WordId`,
> `IX_WordPerformances_UserId_NextReview` e `IX_WordPerformances_UserId_LastReview`.

## 4. Seed de dados

- **`DatabaseSeeder`** em `MemoLingo.Infrastructure/Data/Seeding/DatabaseSeeder.cs`:
  idempotente, carrega na ordem idiomas → palavras → frases → vínculos frase/palavra
  → cursos → lições → vínculos lição/palavra.
- **Models de seed** em `MemoLingo.Infrastructure/Data/Seeding/Models/`.
- **Dataset JSON** em `Documents/Seed/` (um arquivo por tabela): `languages.json`,
  `words.json`, `sentences.json`, `sentence-words.json`, `courses.json`,
  `lessons.json`, `lesson-words.json`.
- Arquivos usam **chaves naturais** (código do idioma, texto da palavra, nome do
  curso), não ids.
- Configurável por `Seed:Enabled` e `Seed:Path` (env: `Seed__Enabled`, `Seed__Path`).

## 5. API

- `UsersController` (`MemoLingo.Api/Controllers/UsersController.cs`) com CRUD de
  usuários e `ProducesResponseType` documentado.
- `ErrorResponseModel` como contrato padrão de erro.
- `IUserService` / `UserService` em `MemoLingo.Application/Services/`.
- OpenAPI/Swagger habilitado em desenvolvimento; geração do cliente configurada em
  `MemoLingo.Api/nswag.json`.

## 6. Frontend

- Layout base (`MainLayout`, `NavMenu`) e roteamento (`App.razor`, `NotFound.razor`).
- **Tela inicial com trilha de lições** estilo mapa do Duolingo: `Pages/Learn.razor`.
- **Tela de "palavras em foco"**: `Pages/Praticar.razor`.
- Tela de perfil (esqueleto): `Pages/Perfil.razor`.
- Serviços de dados mockados: `Services/LessonService.cs`, `Services/WordService.cs`.
- Models de apoio: `Models/Lesson.cs`, `Unit.cs`, `Word.cs`, `LessonStatus.cs`,
  `LessonType.cs`.

## 7. Documentação

- [`README.md`](../README.md): conceito, arquitetura, estrutura, como rodar e seed.
- [`Documents/domain-model.md`](./domain-model.md): diagrama ER do domínio.
- [`TODO.md`](../TODO.md): roteiro do que ainda falta.
- Este arquivo: histórico do que já foi entregue.
