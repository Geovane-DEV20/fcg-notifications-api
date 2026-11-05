# FCG Notifications API

Microsserviço responsável por enviar notificações (simuladas por logs) para usuários da plataforma FIAP Cloud Games.

## Funcionalidades

- Consumir evento `UserCreatedEvent` e "enviar" email de boas-vindas
- Consumir evento `PaymentProcessedEvent` e "enviar" email de confirmação de compra (se aprovado)
- Consultar histórico de notificações enviadas

## Tecnologias

- .NET 8
- ASP.NET Core Web API
- Entity Framework Core (InMemory Database)
- JWT Bearer Authentication
- RabbitMQ.Client
- Swagger/OpenAPI

## Variáveis de Ambiente

- `Jwt__Secret`: Chave secreta para validação do JWT
- `RabbitMQ__Host`: Hostname do servidor RabbitMQ (padrão: localhost)
- `RabbitMQ__Port`: Porta do RabbitMQ (padrão: 5672)
- `RabbitMQ__Username`: Usuário do RabbitMQ (padrão: guest)
- `RabbitMQ__Password`: Senha do RabbitMQ (padrão: guest)

## Como Executar

### Com .NET CLI

```bash
dotnet restore
dotnet build
dotnet run
```

### Com Docker

```bash
docker build -t fcg-notifications-api .
docker run -p 8080:8080 fcg-notifications-api
```

## Endpoints

- `GET /api/notifications` - Listar notificações do usuário autenticado

## Eventos Consumidos

### UserCreatedEvent
**Routing Key:** `user.created`

```json
{
  "userId": "guid",
  "nome": "string",
  "email": "string",
  "timestamp": "datetime"
}
```

### PaymentProcessedEvent
**Routing Key:** `payment.processed`

```json
{
  "orderId": "guid",
  "userId": "guid",
  "gameId": "guid",
  "price": 299.90,
  "status": "Approved",
  "timestamp": "datetime"
}
```

## Simulação de Notificações

O serviço simula o envio de emails registrando logs detalhados:
- Email de boas-vindas ao cadastrar usuário
- Email de confirmação de compra quando pagamento é aprovado
- Todas as notificações são armazenadas no banco de dados para consulta
