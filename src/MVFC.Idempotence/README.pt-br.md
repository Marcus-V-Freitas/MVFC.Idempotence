# MVFC.Idempotence

Uma biblioteca de idempotência leve e eficiente para Minimal APIs do ASP.NET Core, baseada em Redis.

[![CI](https://github.com/Marcus-V-Freitas/MVFC.Idempotence/actions/workflows/ci.yml/badge.svg)](https://github.com/Marcus-V-Freitas/MVFC.Idempotence/actions/workflows/ci.yml)
[![codecov](https://codecov.io/gh/Marcus-V-Freitas/MVFC.Idempotence/branch/main/graph/badge.svg)](https://codecov.io/gh/Marcus-V-Freitas/MVFC.Idempotence)
[![License](https://img.shields.io/badge/license-Apache--2.0-blue)](LICENSE)
![Platform](https://img.shields.io/badge/.NET-9%20%7C%2010-blue)
![NuGet Version](https://img.shields.io/nuget/v/MVFC.Idempotence)
![NuGet Downloads](https://img.shields.io/nuget/dt/MVFC.Idempotence)

[English README](README.md)

---

## Visão Geral

`MVFC.Idempotence` garante que requisições HTTP idênticas sejam processadas apenas uma vez, evitando efeitos colaterais indesejados de retentativas ou envios duplicados. Ela fornece uma experiência simples, semelhante a atributos, para Minimal APIs utilizando filtros de endpoint.

Em sistemas distribuídos, a idempotência é essencial. Seja em processamento de pagamentos, criação de pedidos ou qualquer operação de escrita, a mesma requisição deve sempre produzir o mesmo resultado sem executar a lógica de negócio mais de uma vez.

| Pacote | Serviço | Downloads |
|---|---|---|
| [MVFC.Idempotence](src/MVFC.Idempotence/README.md) | Idempotency for Minimal APIs | ![Downloads](https://img.shields.io/nuget/dt/MVFC.Idempotence) |

---

## Funcionalidades

- **Baseado em Redis**: Armazenamento confiável para chaves de idempotência e respostas em cache usando StackExchange.Redis.
- **Configuração Fluente**: Configuração fácil dentro do builder de serviços do ASP.NET Core.
- **Integração com Minimal API**: Projetada nativamente para o estilo moderno `.MapPost`, `.MapPut`, etc.
- **Resolução de Chave Flexível**: Resolva chaves de idempotência a partir de cabeçalhos, valores de rota ou lógica personalizada.
- **TTL Configurável**: Controle por quanto tempo os resultados em cache permanecem no Redis.
- **Suporte a Grupos**: Aplique idempotência a grupos inteiros de rotas de uma só vez.

---

## Instalação

Via .NET CLI:

```bash
dotnet add package MVFC.Idempotence
```

Ou via NuGet Package Manager:

```bash
Install-Package MVFC.Idempotence
```

---

## Configuração

### 1. Registrar Serviços

No seu `Program.cs`, adicione os serviços de idempotência e aponte para a string de conexão do Redis.

```csharp
var builder = WebApplication.CreateBuilder(args);
var redisConnection = builder.Configuration.GetConnectionString("redis")!;

builder.Services.AddIdempotencyRedis(redisConnection, cfg =>
{
    cfg.Ttl = TimeSpan.FromHours(24);       // TTL padrão para resultados em cache
    cfg.HeaderName = "X-Idempotency-Key";   // Cabeçalho padrão para buscar as chaves
    cfg.AllowedMethods = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "POST", "PUT", "PATCH" };
});
```

---

## Exemplos de Uso

Habilite a idempotência em endpoints específicos usando os métodos de extensão `.WithIdempotency()`.

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

Se a chave de idempotência faz parte da URL (comum para operações `PUT`), especifique o nome do parâmetro de rota.

```csharp
app.MapPut("/api/orders/{orderId:guid}", async (Guid orderId, UpdateOrderRequest req) =>
{
    // Este endpoint é idempotente baseado no valor da rota {orderId}
    return Results.Ok(new { orderId, Status = "Updated" });
}).WithIdempotency(fromRoute: "orderId");
```

### Resolutor de Chave Customizado

Para cenários mais complexos, forneça uma função personalizada para resolver a chave a partir do `HttpContext`.

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

Aplique idempotência a um grupo inteiro de endpoints. Útil quando todos os endpoints em um determinado prefixo devem seguir a mesma regra de idempotência.

```csharp
var orders = app.MapGroup("/api/orders").WithIdempotency();

orders.MapPost("/", CreateOrder);
orders.MapPut("/{id}", UpdateOrder);
```

### Sobrescrevendo TTL por Endpoint

Sobrescreva o TTL ou opções específicas para um único endpoint.

```csharp
app.MapPost("/api/expensive-operation", () => Results.Ok())
   .WithIdempotency(ttl: TimeSpan.FromMinutes(30));
```

---

## Parâmetros de Configuração

| Parâmetro        | Tipo           | Descrição                                                            |
| :--------------- | :------------- | :------------------------------------------------------------------- |
| `Ttl`            | `TimeSpan`     | Tempo que o resultado permanece cacheado no Redis.                   |
| `HeaderName`     | `string`       | O cabeçalho HTTP usado para identificar a chave de idempotência.     |
| `AllowedMethods` | `ISet<string>` | Os métodos HTTP permitidos para idempotência (ex: POST, PUT, PATCH). |

---

## Como Funciona

1. Uma requisição chega em um endpoint idempotente.
2. A biblioteca extrai a chave de idempotência (do header, da rota ou de um resolutor customizado).
3. Ela verifica no Redis se já existe uma resposta cacheada para aquela chave.
4. **Se existir resposta em cache**, ela é retornada imediatamente — a lógica de negócio **não** é executada novamente.
5. **Se não existir**, a requisição segue normalmente e a resposta é armazenada no Redis com o TTL configurado.

---

## Integrações

O `MVFC.Idempotence` foi construído para funcionar perfeitamente com:

- **StackExchange.Redis**: O cliente subjacente para comunicação com Redis.
- **ASP.NET Core Minimal APIs**: Suporte nativo para filtros de endpoint.
- **Distributed Cache**: Utiliza abstrações padrão de cache onde possível.

---

## Estrutura do Projeto

- **[src](src)**: Código-fonte principal da biblioteca `MVFC.Idempotence`.
- **[playground](playground)**: Ambiente de teste e demonstração (API de exemplo) para validar a implementação.
- **[tests](tests)**: Conjunto de testes para garantir estabilidade e comportamento esperado.

---

## Changelog

Veja o [CHANGELOG.md](CHANGELOG.md) para o histórico de alterações e versões.

---

## Contribuindo

Veja [CONTRIBUTING.md](CONTRIBUTING.md).

## Licença

[Apache-2.0](LICENSE)
