# Modelo de domínio — MemoLingo

Diagrama das entidades existentes hoje em `MemoLingo.Domain/Entities` e seus relacionamentos.

```mermaid
erDiagram
	LANGUAGE ||--o{ USER : "é idioma nativo de"
	LANGUAGE ||--o{ LANGUAGE_PROGRESS : "é aprendido em"
	LANGUAGE ||--o{ WORD : "possui"
	LANGUAGE ||--o{ SENTENCE : "possui"
	LANGUAGE ||--o{ COURSE : "possui"
	LANGUAGE ||--o{ STUDY_SESSION : "é estudado em"
	USER ||--o{ LANGUAGE_PROGRESS : "possui"
	USER ||--o{ WORD_PERFORMANCE : "possui"
	USER ||--o{ STUDY_SESSION : "realiza"
	USER ||--o{ USER_NODE_PROGRESS : "possui"
	COURSE ||--o{ SECTION : "contém"
	SECTION ||--o{ UNIT : "contém"
	UNIT ||--o{ PATH_NODE : "contém"
	PATH_NODE ||--o{ LESSON : "contém"
	PATH_NODE ||--o{ USER_NODE_PROGRESS : "é rastreado em"
	LESSON ||--o{ CHALLENGE : "contém"
	LESSON ||--o{ LESSON_WORD : "trabalha"
	LESSON ||--o{ STUDY_SESSION : "é praticada em"
	CHALLENGE ||--o{ EXERCISE_ATTEMPT : "é respondido em"
	WORD ||--o{ LESSON_WORD : "é trabalhada em"
	WORD ||--o{ WORD_PERFORMANCE : "é avaliada em"
	WORD ||--o{ SENTENCE_WORD : "aparece em"
	WORD ||--o{ CHALLENGE : "é exercitada em"
	WORD ||--o{ EXERCISE_ATTEMPT : "é exercitada em"
	SENTENCE ||--o{ SENTENCE_WORD : "contém"
	SENTENCE ||--o{ CHALLENGE : "é exercitada em"
	SENTENCE ||--o{ EXERCISE_ATTEMPT : "é exercitada em"
	STUDY_SESSION ||--o{ EXERCISE_ATTEMPT : "registra"

	LANGUAGE {
		int Id PK
		string Name
		string Code UK
	}

	USER {
		int Id PK
		string Name
		string Email UK
		string PasswordHash
		DateTime CreatedAt
		bool Active
		int NativeLanguageId FK
	}

	LANGUAGE_PROGRESS {
		int Id PK
		int UserId FK
		int LanguageId FK
		int Level
		int TotalXp
		bool IsActiveCourse
		int TotalLearnedWords
		int TotalCompletedLessons
		int CurrentStreakDays
		DateTime CreatedAt
	}

	WORD {
		int Id PK
		int LanguageId FK
		string Text
		string Translation
		CefrLevel CefrLevel
		PartOfSpeech PartOfSpeech
	}

	SENTENCE {
		int Id PK
		int LanguageId FK
		string Text
		string Translation
		CefrLevel CefrLevel
	}

	SENTENCE_WORD {
		int Id PK
		int SentenceId FK
		int WordId FK
		int Position
	}

	COURSE {
		int Id PK
		int LanguageId FK
		string Name
		string Description
		int Position
		CefrLevel CefrLevel
		bool Active
	}

	SECTION {
		int Id PK
		int CourseId FK
		string Title
		string Description
		int Position
		CefrLevel CefrLevel
		bool Active
	}

	UNIT {
		int Id PK
		int SectionId FK
		string Title
		string Topic
		string GuidebookMarkdown
		int Position
		bool Active
	}

	PATH_NODE {
		int Id PK
		int UnitId FK
		NodeType NodeType
		int Position
		int TotalLessons
		bool Active
	}

	LESSON {
		int Id PK
		int PathNodeId FK
		int Position
		int XpReward
		bool Active
	}

	CHALLENGE {
		int Id PK
		int LessonId FK
		int WordId FK "nulo"
		int SentenceId FK "nulo"
		ExerciseType ExerciseType
		string Prompt
		string ExpectedAnswer
		string OptionsJson
		int Position
		bool Active
	}

	USER_NODE_PROGRESS {
		int Id PK
		int UserId FK
		int PathNodeId FK
		int CompletedLessonsCount
		bool IsCompleted
		DateTime CompletedAt "nulo"
		DateTime LastPracticedAt "nulo"
	}

	LESSON_WORD {
		int Id PK
		int LessonId FK
		int WordId FK
		int Position
	}

	STUDY_SESSION {
		int Id PK
		int UserId FK
		int LanguageId FK
		int LessonId FK "nulo"
		ProgressStatus Status
		DateTime StartedAt
		DateTime FinishedAt "nulo"
		int CorrectCount
		int WrongCount
		int XpEarned
	}

	EXERCISE_ATTEMPT {
		int Id PK
		int StudySessionId FK
		int ChallengeId FK "nulo"
		int WordId FK "nulo"
		int SentenceId FK "nulo"
		ExerciseType ExerciseType
		string GivenAnswer
		string ExpectedAnswer
		bool IsCorrect
		int ResponseTimeMs
		DateTime AnsweredAt
	}

	WORD_PERFORMANCE {
		int Id PK
		int UserId FK
		int WordId FK
		int StrengthLevel
		int CorrectCount
		int WrongCount
		DateTime LastReview "nulo"
		DateTime NextReview
	}
```

## Enums

- `CefrLevel`: `A1`, `A2`, `B1`, `B2`, `C1`, `C2`.
- `PartOfSpeech`: `Noun`, `Verb`, `Adjective`, `Adverb`, `Pronoun`, `Preposition`, `Conjunction`, `Interjection`, `Article`, `Numeral`, `Expression`.
- `ExerciseType`: `MultipleChoice`, `FreeTranslation`, `FillInTheBlank`, `ListenAndWrite`, `Speaking`, `WordOrdering`.
- `NodeType`: `Skill`, `Story`, `Practice`, `Chest`.
- `ProgressStatus`: `Locked`, `Available`, `InProgress`, `Completed`, `Abandoned`.

## Observações

- `User.NativeLanguageId` → `Language` com `DeleteBehavior.Restrict`.
- `LanguageProgress` é a entidade de associação entre `User` e `Language` (índice único em `UserId` + `LanguageId`); exclusão em cascata a partir de `User` e restrita a partir de `Language`.
- `Word` pertence a um `Language` por meio da propriedade de navegação `Language` e possui nível CEFR (`CefrLevel`) e categoria gramatical (`PartOfSpeech`).
- `Sentence` pertence a um `Language` e associa-se a `Word` por meio da entidade de junção `SentenceWord` (com `Position` indicando a ordem da palavra na frase).
- `Course` agrupa uma trilha de um `Language`, ordenada por `Position`, e é dividido em `Section` (exclusão em cascata a partir de `Course`).
- A hierarquia da trilha é `Course` → `Section` → `Unit` → `PathNode` → `Lesson` → `Challenge`, sempre em cascata do pai para o filho.
- `Unit` concentra o `Title`, o `Topic` e o `GuidebookMarkdown` exibidos na trilha; `PathNode` representa a "bolinha" do mapa e define quantas lições são necessárias para concluí-la (`TotalLessons`).
- `Lesson` associa-se a `Word` por meio da entidade de junção `LessonWord` (índice único em `LessonId` + `WordId`) e contém os `Challenge` executados na prática.
- `UserNodeProgress` rastreia quantas lições o usuário concluiu em um `PathNode` (índice único em `UserId` + `PathNodeId`).
- `StudySession` representa uma prática do usuário em um idioma, opcionalmente vinculada a uma `Lesson` (`LessonId` nulo, com `DeleteBehavior.SetNull`), e acumula `CorrectCount`, `WrongCount` e `XpEarned`.
- `ExerciseAttempt` pertence a uma `StudySession` (exclusão em cascata) e referencia opcionalmente o `Challenge` respondido, além de `Word` ou `Sentence`, conforme o `ExerciseType`.
- `WordPerformance` referencia `User` e `Word` por navegação e registra acertos (`CorrectCount`), erros (`WrongCount`), a última revisão (`LastReview`, nula enquanto a palavra não for revisada) e a próxima revisão (`NextReview`).
- Todas as entidades estão mapeadas como `DbSet` em `AppDbContext`: `Users`, `Languages`, `LanguageProgresses`, `Words`, `Sentences`, `SentenceWords`, `Courses`, `Sections`, `Units`, `PathNodes`, `Lessons`, `Challenges`, `LessonWords`, `UserNodeProgresses`, `StudySessions`, `ExerciseAttempts` e `WordPerformances`.
