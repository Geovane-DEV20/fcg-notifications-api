using System.Text.Json;
using FcgNotificationsApi.Data;
using FcgNotificationsApi.Models;

namespace FcgNotificationsApi.Services;

public class EventConsumers : BackgroundService
{
    private readonly IRabbitMqService _rabbitMqService;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EventConsumers> _logger;

    public EventConsumers(
        IRabbitMqService rabbitMqService,
        IServiceProvider serviceProvider,
        ILogger<EventConsumers> logger)
    {
        _rabbitMqService = rabbitMqService;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Consumir UserCreatedEvent
        _rabbitMqService.StartConsuming("user.created", async (message) =>
        {
            try
            {
                var userEvent = JsonSerializer.Deserialize<UserCreatedEvent>(message);
                if (userEvent != null)
                {
                    await SendWelcomeEmail(userEvent);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao processar UserCreatedEvent: {ex.Message}");
            }
        });

        // Consumir PaymentProcessedEvent
        _rabbitMqService.StartConsuming("payment.processed", async (message) =>
        {
            try
            {
                var paymentEvent = JsonSerializer.Deserialize<PaymentProcessedEvent>(message);
                if (paymentEvent != null && paymentEvent.Status == "Approved")
                {
                    await SendPurchaseConfirmation(paymentEvent);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao processar PaymentProcessedEvent: {ex.Message}");
            }
        });

        return Task.CompletedTask;
    }

    private async Task SendWelcomeEmail(UserCreatedEvent userEvent)
    {
        var subject = "Bem-vindo à FIAP Cloud Games!";
        var body = $@"
Olá {userEvent.Nome},

Seja bem-vindo à FIAP Cloud Games!

Sua conta foi criada com sucesso. Explore nosso catálogo de jogos e aproveite!

Atenciosamente,
Equipe FIAP Cloud Games
";

        _logger.LogInformation($"""
        
        ==============================================
        📧 EMAIL DE BOAS-VINDAS ENVIADO
        ==============================================
        Para: {userEvent.Email}
        Nome: {userEvent.Nome}
        Assunto: {subject}
        Mensagem:
        {body}
        ==============================================
        
        """);

        await SaveNotification(userEvent.UserId, userEvent.Email, "Welcome", subject, body);
    }

    private async Task SendPurchaseConfirmation(PaymentProcessedEvent paymentEvent)
    {
        var subject = "Compra Confirmada - FIAP Cloud Games";
        var body = $@"
Olá!

Sua compra foi confirmada com sucesso!

Detalhes da compra:
- Order ID: {paymentEvent.OrderId}
- Game ID: {paymentEvent.GameId}
- Valor: R$ {paymentEvent.Price:F2}
- Status: {paymentEvent.Status}

O jogo foi adicionado à sua biblioteca e já está disponível para jogar.

Atenciosamente,
Equipe FIAP Cloud Games
";

        _logger.LogInformation($"""
        
        ==============================================
        📧 EMAIL DE CONFIRMAÇÃO DE COMPRA ENVIADO
        ==============================================
        Para: Usuário {paymentEvent.UserId}
        Assunto: {subject}
        Mensagem:
        {body}
        ==============================================
        
        """);

        await SaveNotification(paymentEvent.UserId, "user@email.com", "PurchaseConfirmation", subject, body);
    }

    private async Task SaveNotification(Guid userId, string email, string type, string subject, string body)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<NotificationsDbContext>();

        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Email = email,
            Type = type,
            Subject = subject,
            Body = body,
            SentAt = DateTime.UtcNow
        };

        dbContext.Notifications.Add(notification);
        await dbContext.SaveChangesAsync();
    }
}
