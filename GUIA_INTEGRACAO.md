# 🔔 Guia de Integração - Notifications API + Users API

## 🎯 O que foi implementado?

A **fcg-notifications-api** agora **consome eventos** publicados pela **fcg-users-api** via RabbitMQ e envia notificações por email (simulado).

---

## 📊 Arquitetura Implementada

```
┌─────────────────────┐
│  fcg-users-api      │
│  POST /api/usuarios │
└──────────┬──────────┘
           │ 1. Salva no DB
           │ 2. Publica evento
           ▼
    ┌──────────────┐
    │   RabbitMQ   │
    │   Exchange   │
    │  fcg.users   │
    │   .events    │
    └──────┬───────┘
           │ Routing Key: users.created
           ▼
┌──────────────────────────┐
│ fcg-notifications-api    │
│ UsersEventsConsumer      │
│   ↓                      │
│ EmailService             │
│   ↓                      │
│ 📧 Envia Email           │
└──────────────────────────┘
```

---

## 🔄 Eventos Consumidos

| Evento | Routing Key | Ação |
|--------|-------------|------|
| **UsuarioCriadoEvent** | `users.created` | Envia email de boas-vindas |
| **ContaAtivadaEvent** | `users.activated` | Envia email de confirmação |
| **SenhaAlteradaEvent** | `users.password.changed` | Envia notificação de alteração |
| **PerfilAlteradoEvent** | `users.profile.changed` | Envia notificação ao usuário |

---

## 🚀 Como Testar (Passo a Passo)

### 1️⃣ Subir RabbitMQ (Docker)

```bash
# Na pasta fcg-users-api
cd "d:\ProjetoFiap\Mcrossserviços NEW FIAP\fcg-users-api"
docker-compose -f docker-compose.rabbitmq.yml up -d
```

**Verificar se está rodando:**
```bash
docker ps | findstr rabbitmq
```

### 2️⃣ Executar fcg-users-api (Terminal 1)

```bash
cd "d:\ProjetoFiap\Mcrossserviços NEW FIAP\fcg-users-api"
dotnet run
```

**Aguardar até ver:**
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:62306
```

### 3️⃣ Executar fcg-notifications-api (Terminal 2)

```bash
cd "d:\ProjetoFiap\Mcrossserviços NEW FIAP\fcg-notifications-api"
dotnet run
```

**Aguardar até ver:**
```
info: FcgNotificationsApi.Consumers.UsersEventsConsumer[0]
      🚀 UsersEventsConsumer iniciando...
info: FcgNotificationsApi.Consumers.UsersEventsConsumer[0]
      🔗 RabbitMQ conectado | Exchange: fcg.users.events.dev | Queue: notifications.users.dev
info: FcgNotificationsApi.Consumers.UsersEventsConsumer[0]
      ✅ UsersEventsConsumer rodando. Aguardando eventos...
```

### 4️⃣ Cadastrar Usuário via Swagger

**Abrir:** http://localhost:62306/swagger

**POST /api/usuarios:**
```json
{
  "nome": "João Silva",
  "email": "joao@example.com",
  "senha": "Senha123!",
  "perfil": "Usuario"
}
```

### 5️⃣ Ver Logs em Tempo Real

**Terminal 1 (fcg-users-api):**
```
info: fcg_users_api.Services.CadastroService[0]
      Usuário cadastrado com sucesso. ID: ..., Email: joao@example.com
info: fcg_users_api.Events.RabbitMQ.RabbitMQEventPublisher[0]
      Evento publicado: UsuarioCriado | ID: ... | RoutingKey: users.created
```

**Terminal 2 (fcg-notifications-api):** 🎉
```
info: FcgNotificationsApi.Consumers.UsersEventsConsumer[0]
      📩 Evento recebido | RoutingKey: users.created | MessageId: ...
info: FcgNotificationsApi.Consumers.UsersEventsConsumer[0]
      👤 Usuário criado | UsuarioId: ... | Nome: João Silva | Email: joao@example.com
info: FcgNotificationsApi.Services.EmailService[0]
      📧 [EMAIL] Enviando email de boas-vindas | Para: joao@example.com | Nome: João Silva
info: FcgNotificationsApi.Services.EmailService[0]
      ✅ [EMAIL ENVIADO] Assunto: Bem-vindo à FIAP Cloud Games! | Para: joao@example.com
info: FcgNotificationsApi.Consumers.UsersEventsConsumer[0]
      ✅ Evento processado com sucesso | RoutingKey: users.created
```

---

## 📧 Exemplo de Email (Simulado)

**Para:** joao@example.com  
**Assunto:** Bem-vindo à FIAP Cloud Games!

```
Olá João Silva!

Seja bem-vindo(a) à FIAP Cloud Games! 🎮

Sua conta foi criada com sucesso. Para começar a usar nossos serviços, 
você precisa confirmar seu email clicando no link de ativação que 
enviamos separadamente.

Após a ativação, você terá acesso a:
- Catálogo completo de jogos
- Sua biblioteca pessoal
- Sistema de pagamentos

Qualquer dúvida, estamos à disposição!

Equipe FIAP Cloud Games
```

---

## 🔍 Monitoramento no RabbitMQ Management

**Acessar:** http://localhost:15672  
**Usuário/Senha:** `guest` / `guest`

### Ver Exchange
1. Aba **Exchanges**
2. Clicar em `fcg.users.events.dev`
3. Ver bindings e estatísticas

### Ver Fila
1. Aba **Queues**
2. Clicar em `notifications.users.dev`
3. Ver mensagens consumidas, taxa de processamento, etc.

### Ver Mensagens (Debug)
1. Aba **Queues** → `notifications.users.dev`
2. Expandir **Get messages**
3. Clicar em **Get Message(s)**

---

## 🧪 Testar Todos os Eventos

### 1. Cadastrar Usuário
```http
POST http://localhost:62306/api/usuarios
{
  "nome": "Maria Santos",
  "email": "maria@example.com",
  "senha": "Senha123!",
  "perfil": "Usuario"
}
```
✅ **Resultado:** Email de boas-vindas

### 2. Ativar Conta
```http
POST http://localhost:62306/api/usuarios/ativacao
{
  "token": "TOKEN_OBTIDO_NO_RESPONSE"
}
```
✅ **Resultado:** Email de confirmação

### 3. Alterar Senha
```http
PUT http://localhost:62306/api/usuarios/{id}/senha
Authorization: Bearer {TOKEN}
{
  "senhaAtual": "Senha123!",
  "novaSenha": "NovaSenha456!"
}
```
✅ **Resultado:** Email de notificação de alteração

### 4. Alterar Perfil (Admin)
```http
PATCH http://localhost:62306/api/usuarios/{id}/perfil
Authorization: Bearer {TOKEN_ADMIN}
{
  "perfil": "Administrador"
}
```
✅ **Resultado:** Email de notificação de mudança de perfil

---

## ⚙️ Configuração

### appsettings.Development.json
```json
{
  "RabbitMQ": {
    "HostName": "localhost",
    "Port": "5672",
    "UserName": "guest",
    "Password": "guest",
    "VirtualHost": "/",
    "ExchangeName": "fcg.users.events.dev",
    "QueueName": "notifications.users.dev"
  }
}
```

**Em produção:**
- Alterar `HostName` para servidor RabbitMQ dedicado
- Usar variáveis de ambiente para credenciais
- Exchange sem sufixo `.dev`

---

## 🛠️ Estrutura de Arquivos

```
fcg-notifications-api/
├── Consumers/
│   └── UsersEventsConsumer.cs      # Background Service (consumer)
├── Events/
│   └── DomainEvents/
│       ├── IDomainEvent.cs
│       ├── UsuarioCriadoEvent.cs
│       ├── ContaAtivadaEvent.cs
│       ├── SenhaAlteradaEvent.cs
│       └── PerfilAlteradoEvent.cs
├── Services/
│   ├── IEmailService.cs
│   └── EmailService.cs              # Simulação de envio de email
├── Controllers/
│   └── NotificationsController.cs
├── Program.cs                       # Configuração do consumer
├── appsettings.json
└── appsettings.Development.json
```

---

## 🚨 Troubleshooting

### Consumer não conecta ao RabbitMQ

**Erro:**
```
❌ Erro ao inicializar RabbitMQ. Consumer não foi iniciado.
```

**Solução:**
```bash
# Verificar se RabbitMQ está rodando
docker ps | findstr rabbitmq

# Se não estiver, subir novamente
cd "d:\ProjetoFiap\Mcrossserviços NEW FIAP\fcg-users-api"
docker-compose -f docker-compose.rabbitmq.yml up -d
```

### Consumer não recebe mensagens

**Causas comuns:**
1. Exchange diferente entre APIs (verificar `.dev` no nome)
2. Routing keys incorretas no bind
3. Fila não criada corretamente

**Verificar no RabbitMQ Management:**
- Exchange: `fcg.users.events.dev` existe?
- Queue: `notifications.users.dev` existe?
- Bindings: Routing keys `users.created`, etc. estão configuradas?

### Mensagens sendo reprocessadas infinitamente

**Causa:** Erro no processamento causa NACK com requeue

**Solução:** Verificar logs de erro no Terminal 2 e corrigir o problema

---

## 📚 Próximos Passos

### Fase 1: Envio Real de Emails
- [ ] Integrar com SendGrid, AWS SES ou Azure Communication Services
- [ ] Templates HTML profissionais
- [ ] Tracking de abertura/clique

### Fase 2: Resiliência
- [ ] Dead Letter Queue (DLQ) para mensagens com falha
- [ ] Retry policy com exponential backoff
- [ ] Circuit breaker

### Fase 3: Observabilidade
- [ ] Métricas customizadas (emails enviados, falhas, latência)
- [ ] Alertas (fila crescendo, falhas frequentes)
- [ ] Tracing distribuído (OpenTelemetry)

---

## ✅ Checklist de Validação

- [x] RabbitMQ rodando
- [x] fcg-users-api publicando eventos
- [x] fcg-notifications-api consumindo eventos
- [x] Logs mostrando eventos recebidos
- [x] Logs mostrando emails enviados (simulado)
- [x] RabbitMQ Management mostrando estatísticas

---

**Implementado em:** 21/10/2026  
**Versão:** 1.0  
**Integração:** fcg-users-api v1.1.0 + fcg-notifications-api v1.0
