# MVFC.Idempotence

Uma biblioteca de idempotência leve e eficiente para Minimal APIs do ASP.NET Core, baseada em Redis.

## Objetivo

O objetivo desta biblioteca é garantir que requisições idênticas sejam processadas apenas uma vez, evitando efeitos colaterais indesejados de retentativas ou envios duplicados. Ela fornece uma experiência simples, semelhante a atributos, para Minimal APIs usando filtros de endpoint.

---

## Funcionalidades

- **Baseado em Redis**: Armazenamento confiável para chaves de idempotência e respostas em cache.
- **Configuração Fluente**: Configuração fácil dentro do builder do ASP.NET Core.
- **Integração com Minimal API**: Projetada especificamente para o estilo moderno de `.MapPost`, `.MapPut`, etc.
- **Resolução de Chave Flexível**: Resolva chaves de idempotência a partir de cabeçalhos (headers), valores de rota ou lógica personalizada.
- **TTL Configurável**: Controle por quanto tempo os resultados em cache permanecem no Redis.

---

## Instalação

```bash
dotnet add package MVFC.Idempotence
```

---

## Configuração

### 1. Registrar Serviços

No seu `Program.cs`, adicione os serviços de idempotência e aponte para sua string de conexão do Redis.

```csharp
var builder = WebApplication.CreateBuilder(args);
var redisConnection = builder.Configuration.GetConnectionString("redis")!;

builder.Services.AddIdempotencyRedis(redisConnection, cfg =>
{
    cfg.Ttl = TimeSpan.FromHours(24); // TTL padrão para resultados em cache
    cfg.HeaderName = "X-Idempotency-Key"; // Cabeçalho padrão para buscar as chaves
    cfg.AllowedMethods = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "POST", "PUT", "PATCH" };
});
```

---

## Exemplos de Uso

Você pode habilitar a idempotência em endpoints específicos usando os métodos de extensão `.WithIdempotency()`.

### Uso Básico (Baseado em Header)

Por padrão, a biblioteca procura a chave no `HeaderName` configurado (ex: `X-Idempotency-Key`).

```csharp
app.MapPost("/api/orders", async (CreateOrderRequest req) =>
{
    // Sua lógica aqui
    return Results.Created($"/api/orders/{Guid.NewGuid()}", new { req.ProductId });
}).WithIdempotency();
```

### Idempotência Baseada em Rota

Se a sua chave de idempotência faz parte da URL (comum para operações `PUT`), você pode especificar o nome do parâmetro de rota.

```csharp
app.MapPut("/api/orders/{orderId:guid}", async (Guid orderId, UpdateOrderRequest req) =>
{
    // Este endpoint agora é idempotente baseado no valor da rota {orderId}
    return Results.Ok(new { orderId, Status = "Updated" });
}).WithIdempotency(fromRoute: "orderId");
```

### Resolutor de Chave Customizado

Para cenários mais complexos, você pode fornecer uma função personalizada para resolver a chave a partir do `HttpContext`.

```csharp
app.MapPost("/api/payments", async (PaymentRequest req) => 
{
    return Results.Accepted();
}).WithIdempotency(ctx => 
{
    // Lógica customizada para obter a chave de qualquer lugar no contexto
    return ctx.Request.Headers["Custom-Key"].FirstOrDefault();
});
```

### Uso em Grupos

A idempotência também pode ser aplicada a um grupo inteiro de endpoints. Isso é útil quando todos os endpoints em um determinado prefixo devem seguir a mesma regra de idempotência.

```csharp
var orders = app.MapGroup("/api/orders").WithIdempotency();

orders.MapPost("/", CreateOrder);
orders.MapPut("/{id}", UpdateOrder);
```

### Sobrescrevendo Configurações por Endpoint

Você também pode sobrescrever o TTL ou opções específicas para um único endpoint.

```csharp
app.MapPost("/api/expensive-operation", () => Results.Ok())
   .WithIdempotency(ttl: TimeSpan.FromMinutes(30));
```

---

## Parâmetros de Configuração

| Parâmetro        | Tipo           | Descrição                                                        |
| :--------------- | :------------- | :--------------------------------------------------------------- |
| `Ttl`            | `TimeSpan`     | Tempo que o resultado permanece cacheado no Redis.               |
| `HeaderName`     | `string`       | O cabeçalho HTTP usado para identificar a chave de idempotência. |
| `AllowedMethods` | `ISet<string>` | Os métodos HTTP permitidos para idempotência (ex: POST, PUT).    |

---

## Integrações

O `MVFC.Idempotence` foi construído para funcionar perfeitamente com:
- **StackExchange.Redis**: O cliente subjacente para comunicação com Redis.
- **ASP.NET Core Minimal APIs**: Suporte nativo para filtros de endpoint.
- **Distributed Cache**: Utiliza abstrações padrão de cache onde possível.

---

## Estrutura do Projeto

Este repositório está organizado da seguinte forma:

- **[src](src)**: Contém o código-fonte principal da biblioteca `MVFC.Idempotence`.
- **[playground](playground)**: Ambiente de teste e demonstração (API de exemplo) para validar a implementação da idempotência.
- **[tests](tests)**: Conjunto de testes para garantir a estabilidade e o comportamento esperado da biblioteca.

---

## Licença

Este projeto é licenciado sob a **Apache License 2.0**. Consulte o arquivo [LICENSE](LICENSE) para mais detalhes.