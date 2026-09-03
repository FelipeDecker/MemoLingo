# TODO — Roteiro para um MemoLingo "nível Duolingo"

Legenda: [ ] pendente · [x] concluído

## Fase 0 — Fundação do projeto
- [x] Criar projeto Blazor WebAssembly (`MemoLingo.Front`)
- [x] Criar projeto de backend `MemoLingo.Api` (ASP.NET Core Web API, .NET 10)
- [x] Criar projeto `MemoLingo.Domain` (entidades e regras de negócio puras)
- [x] Configurar solução (.slnx) com todos os projetos
- [ ] Configurar Docker Compose (Api + banco de dados) para desenvolvimento local
- [ ] Configurar CI básico (build + testes) via GitHub Actions

## Fase 1 — Modelagem de domínio
- [x] Modelar entidade `Idioma` (idioma de origem / idioma alvo)
- [x] Modelar entidade `Palavra` (texto, tradução, idioma)
- [x] Complementar entidade `Palavra` com nível/CEFR e categoria gramatical
- [x] Modelar entidade `Frase` (texto, tradução, idioma, lista de palavras associadas)
- [x] Modelar entidade `Licao` (conjunto de exercícios de um tópico)
- [x] Modelar entidade `Trilha`/`Curso` (sequência de lições, ex.: "Básico", "Viagem")
- [x] Modelar entidade `Usuario`
- [x] Modelar entidade `ProgressoUsuarioIdioma` (`LanguageProgress`: nível, XP, streak, curso ativo, totais)
- [x] Modelar entidade `ProgressoPalavra` (usuário x palavra: erros, próxima revisão, nível de domínio)
- [x] Complementar entidade `ProgressoPalavra` com acertos e última revisão
- [x] Modelar entidade `TentativaExercicio` (log de cada resposta: certo/errado, tempo de resposta, timestamp)
- [x] Modelar entidade `SessaoDeEstudo` (agrupa exercícios respondidos em uma sessão)
- [x] Definir enums: tipo de exercício (múltipla escolha, tradução livre, completar frase, ouvir e escrever, falar), status de progresso

## Fase 2 — Banco de dados
- [ ] Escolher banco definitivo (SQLite para dev / PostgreSQL ou SQL Server para produção)
- [x] Criar `DbContext` com EF Core
- [x] Criar migrations iniciais
- [ ] Popular seed de dados: idiomas, palavras e frases iniciais (dataset inicial de 1 idioma)
- [ ] Criar índices para consultas de priorização (usuário + palavra + última revisão)

## Fase 3 — Algoritmo de priorização de erros (o coração do app)
- [ ] Definir métrica de "dificuldade" por palavra (ex.: taxa de erro ponderada por recência — decaimento exponencial)
- [ ] Implementar repetição espaçada (SRS) tipo SM-2/Leitner adaptado, combinando:
  - Intervalo de revisão padrão (baseado em acertos consecutivos)
  - Peso extra para palavras erradas recentemente (reduz intervalo, aumenta frequência)
- [ ] Implementar serviço `PriorizacaoService`: dado um usuário, retorna lista ordenada de palavras candidatas a reforço
- [ ] Implementar serviço `SelecaoDeFrasesService`: dado um conjunto de palavras prioritárias, seleciona/gera frases que as contenham
- [ ] Implementar mistura de conteúdo novo vs. revisão (ex.: 70% revisão de erros / 30% novo conteúdo, configurável)
- [ ] Criar testes unitários do algoritmo de priorização com cenários controlados
- [ ] Adicionar telemetria/logs para validar se o algoritmo está de fato reforçando palavras problemáticas

## Fase 4 — Motor de exercícios
- [ ] Implementar tipo de exercício: múltipla escolha (tradução da palavra)
- [ ] Implementar tipo de exercício: completar a frase (lacuna com a palavra prioritária)
- [ ] Implementar tipo de exercício: traduzir frase completa (digitação livre)
- [ ] Implementar tipo de exercício: ordenar palavras para formar a frase
- [ ] Implementar tipo de exercício: ouvir áudio e escrever (usar Text-to-Speech)
- [ ] Implementar correção automática com tolerância a pequenos erros de digitação/acentuação
- [ ] Implementar feedback imediato (explicação do erro, tradução correta)
- [ ] Registrar cada tentativa em `TentativaExercicio` e atualizar `ProgressoPalavra`

## Fase 5 — API (backend)
- [ ] Endpoint de autenticação (registro/login, JWT)
- [ ] Endpoint: obter próxima lição/sessão personalizada para o usuário
- [ ] Endpoint: submeter resposta de exercício
- [ ] Endpoint: obter estatísticas de progresso do usuário (palavras dominadas, em risco, streak, XP)
- [ ] Endpoint: listar trilhas/cursos disponíveis
- [ ] Validação de entrada e tratamento global de erros
- [ ] Documentação da API (OpenAPI/Swagger)
- [ ] Testes de integração da API

## Fase 6 — Frontend (Blazor WebAssembly)
- [ ] Tela de login/registro
- [x] Tela inicial com trilha de lições (estilo mapa do Duolingo)
- [ ] Tela de exercício (componentizada por tipo de exercício)
- [ ] Tela de resumo de sessão (acertos, erros, XP ganho)
- [x] Tela de "palavras em foco" (mostra ao usuário quais palavras estão sendo reforçadas e por quê)
- [ ] Tela de perfil/estatísticas (progresso geral, streak, histórico)
- [ ] Componente de player de áudio (pronúncia das palavras/frases)
- [ ] Estado global (Fluxor/estado próprio) para sessão de estudo em andamento
- [ ] Consumo da API via `HttpClient` tipado
- [ ] Suporte offline básico (PWA + cache de lição atual)
- [ ] Responsividade mobile-first

## Fase 7 — Gamificação (motivação estilo Duolingo)
- [ ] Sistema de XP por exercício/lição concluída
- [ ] Sistema de streak (dias consecutivos estudando)
- [ ] Sistema de "vidas"/corações (erros limitados por sessão, opcional)
- [ ] Sistema de níveis/ligas (ranking entre usuários)
- [ ] Conquistas/badges (ex.: "dominou 100 palavras", "7 dias seguidos")
- [ ] Notificações de lembrete (push notification / e-mail) para revisar palavras pendentes
- [ ] Metas diárias configuráveis pelo usuário

## Fase 8 — Conteúdo e conteúdo gerado
- [ ] Curadoria de dataset inicial de vocabulário por nível (A1, A2, B1...)
- [ ] Banco de frases de exemplo por palavra/nível
- [ ] (Opcional) Integração com IA para gerar frases contextuais sob demanda contendo as palavras prioritárias
- [ ] (Opcional) Integração com serviço de TTS para pronúncia
- [ ] Pipeline de revisão/qualidade de conteúdo (evitar frases sem sentido)

## Fase 9 — Qualidade, performance e observabilidade
- [ ] Testes unitários (domínio + algoritmo de priorização)
- [ ] Testes de integração (API)
- [ ] Testes end-to-end (fluxo completo de lição no frontend)
- [ ] Logging estruturado e métricas (ex.: Application Insights)
- [ ] Monitoramento de erros (ex.: Sentry/App Insights)
- [ ] Cache de consultas pesadas (ex.: Redis para sessão/priorização)

## Fase 10 — Deploy e operação
- [ ] Pipeline de CI/CD (build, testes, deploy automático)
- [ ] Deploy do backend (Azure App Service/Container Apps)
- [ ] Deploy do frontend (Azure Static Web Apps)
- [ ] Configuração de ambientes (dev/staging/produção)
- [ ] Backups e estratégia de migração de banco de dados
- [ ] Domínio próprio e HTTPS

## Fase 11 — Pós-lançamento / evolução
- [ ] Suporte a múltiplos idiomas de aprendizado simultâneos
- [ ] Exercícios de fala (reconhecimento de voz)
- [ ] Modo "revisão rápida" focado 100% nas piores palavras
- [ ] Exportar relatório de progresso (PDF/CSV)
- [ ] Modo social (amigos, desafios, comparação de progresso)
- [ ] App mobile nativo (MAUI) reutilizando `MemoLingo.Domain`/`Shared`
