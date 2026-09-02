# Instruções para o GitHub Copilot - Projeto Nexa

## Convenções de código C#

- **Namespaces**: sempre utilizar namespace com chaves (block-scoped namespace), nunca file-scoped namespace (`namespace X;`).

  Correto:
  ```csharp
  namespace Nexa.Catalog.Api.Entities
  {
      public class Show
      {
      }
  }
  ```

  Incorreto:
  ```csharp
  namespace Nexa.Catalog.Api.Entities;

  public class Show
  {
  }
  ```

- **Strings nulas**: nunca utilizar `string?`. O projeto não habilita nullable reference types (`Nullable` não está definido como `enable` no csproj), então `string` já é implicitamente anulável. Utilizar sempre `string` em vez de `string?`.

  Correto:
  ```csharp
  public string Descricao { get; init; }
  ```

  Incorreto:
  ```csharp
  public string? Descricao { get; init; }
  ```

- **Entidades**: as propriedades de classes de entidade (pasta `Entities`) não devem ser inicializadas com valores padrão. Apenas declarar o tipo e o acessor. Não deixar linhas em branco entre propriedades; propriedades de navegação ficam por último, separadas por uma única linha em branco.

  Correto:
  ```csharp
  public class Usuario
  {
      public string Nome { get; set; }

      public Language Language { get; set; }
  }
  ```

  Incorreto:
  ```csharp
  public class Usuario
  {
      public string Nome { get; set; } = string.Empty;

      public Language Language { get; set; }
  }
  ```

- **Nomenclatura em inglês**: todos os nomes de classes, métodos, propriedades e variáveis devem ser escritos em inglês.

  Correto:
  ```csharp
  public class User
  {
      public string Name { get; set; }
  }
  ```

  Incorreto:
  ```csharp
  public class Usuario
  {
      public string Nome { get; set; }
  }
  ```

- **Nullable**: nunca criar projetos com `<Nullable>enable</Nullable>` no csproj. Se algum projeto já tiver essa configuração, ela deve ser removida. Também não utilizar `?` em tipos de referência (classes e strings).

- **Serviços**: não adicionar comentários `<summary>` (nem qualquer documentação XML) em classes de serviço nem em interfaces de serviço.

- **Interfaces**: não deixar linha em branco entre as assinaturas dos membros; declarar todos os métodos juntos.

  Correto:
  ```csharp
  public interface IUserService
  {
      Task<UserModel> GetByIdAsync(int id);
      Task<bool> RemoveAsync(int id);
  }
  ```

  Incorreto:
  ```csharp
  public interface IUserService
  {
      Task<UserModel> GetByIdAsync(int id);

      Task<bool> RemoveAsync(int id);
  }
  ```

- **Entidades - espaçamento e ordem das propriedades**: não deixar linha em branco entre as propriedades da entidade. As propriedades de navegação devem ficar por último, depois de todas as propriedades escalares (incluindo as chaves estrangeiras). A única linha em branco permitida é a que separa as propriedades da entidade das propriedades de navegação.

  Correto:
  ```csharp
  public class LanguageProgress
  {
      public int Id { get; set; }
      public int UserId { get; set; }
      public int LanguageId { get; set; }

      public Language Language { get; set; }
      public User User { get; set; }
  }
  ```

  Incorreto:
  ```csharp
  public class LanguageProgress
  {
      public int Id { get; set; }

      public int UserId { get; set; }

      public User User { get; set; }

      public int LanguageId { get; set; }

      public Language Language { get; set; }
  }
  ```

- **Enums**: sempre atribuir explicitamente o valor numérico a cada membro, iniciando sempre em `1`.

  Correto:
  ```csharp
  public enum ProficiencyLevel
  {
      Beginner = 1,
      Intermediate = 2,
      Advanced = 3
  }
  ```

  Incorreto:
  ```csharp
  public enum ProficiencyLevel
  {
      Beginner,
      Intermediate,
      Advanced
  }
  ```
