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

- **Entidades**: as propriedades de classes de entidade (pasta `Entities`) não devem ser inicializadas com valores padrão. Apenas declarar o tipo e o acessor.

  Correto:
  ```csharp
  public class Usuario
  {
      public string Nome { get; set; }
  }
  ```

  Incorreto:
  ```csharp
  public class Usuario
  {
      public string Nome { get; set; } = string.Empty;
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
