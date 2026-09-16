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
	COURSE ||--o{ LESSON : "contém"
	LESSON ||--o{ LESSON_WORD : "trabalha"
	LESSON ||--o{ STUDY_SESSION : "é praticada em"
	WORD ||--o{ LESSON_WORD : "é trabalhada em"
	WORD ||--o{ WORD_PERFORMANCE : "é avaliada em"
	WORD ||--o{ SENTENCE_WORD : "aparece em"
	WORD ||--o{ EXERCISE_ATTEMPT : "é exercitada em"
	SENTENCE ||--o{ SENTENCE_WORD : "contém"
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

	LESSON {
		int Id PK
		int CourseId FK
		string Title
		string Topic
		int Position
		int ExerciseCount
		int XpReward
		CefrLevel CefrLevel
		bool Active
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
- `ProgressStatus`: `Locked`, `Available`, `InProgress`, `Completed`, `Abandoned`.

## Observações

- `User.NativeLanguageId` → `Language` com `DeleteBehavior.Restrict`.
- `LanguageProgress` é a entidade de associação entre `User` e `Language` (índice único em `UserId` + `LanguageId`); exclusão em cascata a partir de `User` e restrita a partir de `Language`.
- `Word` pertence a um `Language` por meio da propriedade de navegação `Language` e possui nível CEFR (`CefrLevel`) e categoria gramatical (`PartOfSpeech`).
- `Sentence` pertence a um `Language` e associa-se a `Word` por meio da entidade de junção `SentenceWord` (com `Position` indicando a ordem da palavra na frase).
- `Course` agrupa uma trilha de um `Language`, ordenada por `Position`, e possui várias `Lesson` (exclusão em cascata a partir de `Course`).
- `Lesson` associa-se a `Word` por meio da entidade de junção `LessonWord` (índice único em `LessonId` + `WordId`).
- `StudySession` representa uma prática do usuário em um idioma, opcionalmente vinculada a uma `Lesson` (`LessonId` nulo, com `DeleteBehavior.SetNull`), e acumula `CorrectCount`, `WrongCount` e `XpEarned`.
- `ExerciseAttempt` pertence a uma `StudySession` (exclusão em cascata) e referencia opcionalmente `Word` ou `Sentence`, conforme o `ExerciseType`.
- `WordPerformance` referencia `User` e `Word` por navegação e registra acertos (`CorrectCount`), erros (`WrongCount`), a última revisão (`LastReview`, nula enquanto a palavra não for revisada) e a próxima revisão (`NextReview`).
- Todas as entidades estão mapeadas como `DbSet` em `AppDbContext`: `Users`, `Languages`, `LanguageProgresses`, `Words`, `Sentences`, `SentenceWords`, `Courses`, `Lessons`, `LessonWords`, `StudySessions`, `ExerciseAttempts` e `WordPerformances`.
