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
