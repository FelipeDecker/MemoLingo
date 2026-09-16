# MemoLingo

MemoLingo é um app de aprendizado de idiomas inspirado no Duolingo, mas com um
diferencial: o foco do aprendizado se adapta às **palavras que você mais erra**.
Em vez de repetir todo o conteúdo de forma uniforme, o MemoLingo prioriza a
exibição de frases contendo as palavras com maior taxa de erro recente, para
acelerar a memorização exatamente do que você tem mais dificuldade.

## 💡 Conceito

- Cada palavra estudada possui um **histórico de acertos e erros**.
- Um **algoritmo de priorização** (baseado em repetição espaçada + peso de
  erros recentes) decide quais palavras precisam aparecer com mais frequência.
- O sistema gera **frases e exercícios** que contêm essas palavras
  prioritárias, misturadas a conteúdo novo, para reforçar o contexto de uso.
- Conforme o desempenho do usuário melhora em uma palavra, sua prioridade cai
  gradualmente, liberando espaço para novas palavras problemáticas.

## 🧱 Arquitetura (planejada)

| Camada | Tecnologia |
|---|---|
| Frontend | Blazor WebAssembly (.NET 10), PWA |
| Backend | ASP.NET Core Web API (.NET 10) |
| Persistência | PostgreSQL via EF Core |
| Autenticação | ASP.NET Core Identity / JWT |
| Hospedagem | Azure App Service / Azure Static Web Apps |

> Estado atual: front (Blazor WebAssembly), API, domínio e infraestrutura já
> existem, com PostgreSQL via Docker Compose e seed inicial de conteúdo. A
> lógica de aprendizado ainda será implementada — veja o [TODO.md](./TODO.md).

## 📂 Estrutura do repositório

```
MemoLingo/
├── MemoLingo.Front/          # Aplicação Blazor WebAssembly (PWA)
├── MemoLingo.Api/            # ASP.NET Core Web API + Dockerfile
├── MemoLingo.Application/    # Serviços e modelos de aplicação
├── MemoLingo.Domain/         # Entidades, enums e contratos de domínio
├── MemoLingo.Infrastructure/ # EF Core (DbContext, migrations, seeder)
├── Documents/                # Modelo de domínio, entregas concluídas e seed
│   └── Seed/                 # Dataset inicial em JSON (um arquivo por tabela)
├── docker-compose.yml        # Api + PostgreSQL para desenvolvimento local
├── MemoLingo.slnx            # Solução do Visual Studio
├── README.md
└── TODO.md                   # Roteiro detalhado até um app "nível Duolingo"
```

## 🚀 Como rodar

### Api + PostgreSQL via Docker Compose

```powershell
docker compose up -d --build
```

- API: `http://localhost:8080` (Swagger em `/swagger`)
- PostgreSQL: `localhost:5432` (banco/usuário/senha: `memolingo`)

Na inicialização a API aplica as migrations e executa o seed automaticamente.

### Somente o banco (API pelo Visual Studio / `dotnet run`)

```powershell
docker compose up -d postgres
dotnet run --project MemoLingo.Api
```

### Frontend

```powershell
cd MemoLingo.Front
dotnet run
```

Acesse a URL exibida no terminal (geralmente `https://localhost:xxxx`).

## 🌱 Seed de dados

O dataset inicial fica em `Documents/Seed`, com **um arquivo JSON por tabela**:

| Arquivo | Conteúdo |
|---|---|
| `languages.json` | Idiomas (português, inglês, espanhol e italiano) |
| `words.json` | Palavras em inglês com tradução, nível CEFR e classe gramatical |
| `sentences.json` | Frases em inglês com tradução e nível CEFR |
| `sentence-words.json` | Vínculo entre frases e palavras |
| `courses.json` | Trilhas/cursos |
| `lessons.json` | Lições de cada trilha |
| `lesson-words.json` | Vínculo entre lições e palavras |

Os arquivos usam **chaves naturais** (código do idioma, texto da palavra, nome do
curso etc.) em vez de ids, e o `DatabaseSeeder` é idempotente: registros já
existentes são ignorados. Para desativar o seed, defina `Seed:Enabled` como
`false` (ou a variável de ambiente `Seed__Enabled=false`).

## 🗺️ Roadmap

O roteiro completo de funcionalidades — trilhas de lições, sistema de erros
ponderados, algoritmo de repetição espaçada, gamificação (XP, streak, vidas),
backend, autenticação, etc. — está detalhado em [TODO.md](./TODO.md).

O histórico do que **já foi entregue** (infraestrutura, domínio, banco, seed,
API, front e documentação) está em
[Documents/entregas-concluidas.md](./Documents/entregas-concluidas.md).

## 🤝 Contribuindo

Este é um projeto pessoal em desenvolvimento. Sugestões e PRs são bem-vindos
seguindo as convenções de código já usadas no projeto (.NET / Blazor).

## 📄 Licença

Ainda não definida.
