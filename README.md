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
| Persistência | Banco relacional (SQL Server / PostgreSQL / SQLite) via EF Core |
| Autenticação | ASP.NET Core Identity / JWT |
| Hospedagem | Azure App Service / Azure Static Web Apps |

> Estado atual: apenas o projeto `MemoLingo.Front` (Blazor WebAssembly, template
> padrão) existe no repositório. As camadas de backend, banco de dados e a
> lógica de aprendizado ainda serão implementadas — veja o [TODO.md](./TODO.md).

## 📂 Estrutura do repositório

```
MemoLingo/
├── MemoLingo.Front/     # Aplicação Blazor WebAssembly (PWA)
├── MemoLingo.slnx       # Solução do Visual Studio
├── README.md
└── TODO.md              # Roteiro detalhado até um app "nível Duolingo"
```

## 🚀 Como rodar (estado atual)

```powershell
cd MemoLingo.Front
dotnet run
```

Acesse a URL exibida no terminal (geralmente `https://localhost:xxxx`).

## 🗺️ Roadmap

O roteiro completo de funcionalidades — trilhas de lições, sistema de erros
ponderados, algoritmo de repetição espaçada, gamificação (XP, streak, vidas),
backend, autenticação, etc. — está detalhado em [TODO.md](./TODO.md).

## 🤝 Contribuindo

Este é um projeto pessoal em desenvolvimento. Sugestões e PRs são bem-vindos
seguindo as convenções de código já usadas no projeto (.NET / Blazor).

## 📄 Licença

Ainda não definida.
