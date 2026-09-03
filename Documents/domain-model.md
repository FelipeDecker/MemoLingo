# Modelo de domínio — MemoLingo

Diagrama das entidades existentes hoje em `MemoLingo.Domain/Entities` e seus relacionamentos.

```mermaid
erDiagram
	LANGUAGE ||--o{ USER : "é idioma nativo de"
	LANGUAGE ||--o{ LANGUAGE_PROGRESS : "é aprendido em"
	LANGUAGE ||--o{ WORD : "possui"
	LANGUAGE ||--o{ SENTENCE : "possui"
	USER ||--o{ LANGUAGE_PROGRESS : "possui"
	USER ||--o{ WORD_PERFORMANCE : "possui"
	WORD ||--o{ WORD_PERFORMANCE : "é avaliada em"
	SENTENCE ||--o{ SENTENCE_WORD : "contém"
	WORD ||--o{ SENTENCE_WORD : "aparece em"

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

	WORD_PERFORMANCE {
		int Id PK
		int UserId FK
		int WordId FK
		int StrengthLevel
		int CorrectCount
		int WrongCount
		DateTime LastReview
		DateTime NextReview
	}
```

## Observações

- `User.NativeLanguageId` → `Language` com `DeleteBehavior.Restrict`.
- `LanguageProgress` é a entidade de associação entre `User` e `Language` (índice único em `UserId` + `LanguageId`); exclusão em cascata a partir de `User` e restrita a partir de `Language`.
- `Word` pertence a um `Language` por meio da propriedade de navegação `Language` e possui nível CEFR (`CefrLevel`) e categoria gramatical (`PartOfSpeech`).
- `Sentence` pertence a um `Language` e associa-se a `Word` por meio da entidade de junção `SentenceWord` (com `Position` indicando a ordem da palavra na frase).
- `WordPerformance` referencia `User` e `Word` por navegação e registra acertos (`CorrectCount`), erros (`WrongCount`), a última revisão (`LastReview`, nula enquanto a palavra não for revisada) e a próxima revisão (`NextReview`).
- Apenas `User`, `Language` e `LanguageProgress` estão mapeados como `DbSet` em `AppDbContext`; `Word`, `Sentence`, `SentenceWord` e `WordPerformance` ainda não foram incluídos no contexto nem nas migrations.
