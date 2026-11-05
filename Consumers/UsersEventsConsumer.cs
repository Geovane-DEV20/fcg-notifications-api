using FcgNotificationsApi.Events.DomainEvents;
using FcgNotificationsApi.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace FcgNotificationsApi.Consumers;

/// <summary>
/// Consumer que escuta eventos da Users API via RabbitMQ
/// </summary>
public class UsersEventsConsumer : BackgroundService
{
    private readonly ILogger<UsersEventsConsumer> _logger;
    private readonly IConfiguration _configuration;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private IConnection? _connection;
    private IModel? _channel;

    public UsersEventsConsumer(
        ILogger<UsersEventsConsumer> logger,
        IConfiguration configuration,
        IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _configuration = configuration;
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("🚀 UsersEventsConsumer iniciando...");

        try
        {
            await InicializarRabbitMQAsync(stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao inicializar RabbitMQ. Consumer não foi iniciado.");
            return;
        }

        _logger.LogInformation("✅ UsersEventsConsumer rodando. Aguardando eventos...");

        // Mantém o consumer ativo
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }

    private async Task InicializarRabbitMQAsync(CancellationToken cancellationToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _configuration["RabbitMQ:HostName"] ?? "localhost",
            Port = int.Parse(_configuration["RabbitMQ:Port"] ?? "5672"),
            UserName = _configuration["RabbitMQ:UserName"] ?? "guest",
            Password = _configuration["RabbitMQ:Password"] ?? "guest",
            VirtualHost = _configuration["RabbitMQ:VirtualHost"] ?? "/",
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
        };

        _connection = factory.CreateConnection("FCG-Notifications-Consumer");
        _channel = _connection.CreateModel();

        var exchangeName = _configuration["RabbitMQ:ExchangeName"] ?? "fcg.users.events";
        var queueName = _configuration["RabbitMQ:QueueName"] ?? "notifications.users";

        // Declara a fila (durable para não perder mensagens)
        _channel.QueueDeclare(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );

        // Bind para receber eventos de usuários criados e contas ativadas
        _channel.QueueBind(queueName, exchangeName, "users.created");
        _channel.QueueBind(queueName, exchangeName, "users.activated");
        _channel.QueueBind(queueName, exchangeName, "users.password.changed");
        _channel.QueueBind(queueName, exchangeName, "users.profile.changed");

        _logger.LogInformation(
            "🔗 RabbitMQ conectado | Exchange: {Exchange} | Queue: {Queue}",
            exchangeName, queueName
        );

        // Configura consumer
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            await ProcessarEventoAsync(ea, cancellationToken);
        };

        // Prefetch: processa 1 mensagem por vez (controle de carga)
        _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

        // Inicia o consumo (autoAck = false para controle manual)
        _channel.BasicConsume(
            queue: queueName,
            autoAck: false,
            consumer: consumer
        );

        await Task.CompletedTask;
    }

    private async Task ProcessarEventoAsync(BasicDeliverEventArgs ea, CancellationToken cancellationToken)
    {
        var routingKey = ea.RoutingKey;
        var messageId = ea.BasicProperties.MessageId;

        try
        {
            var body = ea.Body.ToArray();
            var json = Encoding.UTF8.GetString(body);

            _logger.LogInformation(
                "📩 Evento recebido | RoutingKey: {RoutingKey} | MessageId: {MessageId}",
                routingKey, messageId
            );

            // Processa baseado no tipo de evento
            switch (routingKey)
            {
                case "users.created":
                    await ProcessarUsuarioCriadoAsync(json, cancellationToken);
                    break;

                case "users.activated":
                    await ProcessarContaAtivadaAsync(json, cancellationToken);
                    break;

                case "users.password.changed":
                    await ProcessarSenhaAlteradaAsync(json, cancellationToken);
                    break;

                case "users.profile.changed":
                    await ProcessarPerfilAlteradoAsync(json, cancellationToken);
                    break;

                default:
                    _logger.LogWarning("⚠️ Routing key desconhecida: {RoutingKey}", routingKey);
                    break;
            }

            // Confirma processamento com sucesso (ACK)
            _channel?.BasicAck(ea.DeliveryTag, false);

            _logger.LogInformation(
                "✅ Evento processado com sucesso | RoutingKey: {RoutingKey}",
                routingKey
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "❌ Erro ao processar evento | RoutingKey: {RoutingKey} | MessageId: {MessageId}",
                routingKey, messageId
            );

            // Rejeita e recoloca na fila para retry (NACK com requeue)
            _channel?.BasicNack(ea.DeliveryTag, false, true);
        }
    }

    private async Task ProcessarUsuarioCriadoAsync(string json, CancellationToken cancellationToken)
    {
        var evento = JsonSerializer.Deserialize<UsuarioCriadoEvent>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (evento == null)
        {
            _logger.LogError("❌ Falha ao deserializar UsuarioCriadoEvent");
            return;
        }

        _logger.LogInformation(
            "👤 Usuário criado | UsuarioId: {UsuarioId} | Nome: {Nome} | Email: {Email}",
            evento.UsuarioId, evento.Nome, evento.Email
        );

        // Cria scope para resolver IEmailService (scoped)
        using var scope = _serviceScopeFactory.CreateScope();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

        // Envia email de boas-vindas
        await emailService.EnviarEmailBoasVindasAsync(evento.Email, evento.Nome, evento.UsuarioId);
    }

    private async Task ProcessarContaAtivadaAsync(string json, CancellationToken cancellationToken)
    {
        var evento = JsonSerializer.Deserialize<ContaAtivadaEvent>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (evento == null)
        {
            _logger.LogError("❌ Falha ao deserializar ContaAtivadaEvent");
            return;
        }

        _logger.LogInformation(
            "✅ Conta ativada | UsuarioId: {UsuarioId} | Email: {Email}",
            evento.UsuarioId, evento.Email
        );

        using var scope = _serviceScopeFactory.CreateScope();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

        // Envia email de confirmação
        await emailService.EnviarEmailConfirmacaoAtivacaoAsync(evento.Email, evento.UsuarioId);
    }

    private async Task ProcessarSenhaAlteradaAsync(string json, CancellationToken cancellationToken)
    {
        var evento = JsonSerializer.Deserialize<SenhaAlteradaEvent>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (evento == null)
        {
            _logger.LogError("❌ Falha ao deserializar SenhaAlteradaEvent");
            return;
        }

        _logger.LogInformation(
            "🔐 Senha alterada | UsuarioId: {UsuarioId} | Email: {Email}",
            evento.UsuarioId, evento.Email
        );

        using var scope = _serviceScopeFactory.CreateScope();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

        await emailService.EnviarEmailAlteracaoSenhaAsync(evento.Email, evento.UsuarioId);
    }

    private async Task ProcessarPerfilAlteradoAsync(string json, CancellationToken cancellationToken)
    {
        var evento = JsonSerializer.Deserialize<PerfilAlteradoEvent>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (evento == null)
        {
            _logger.LogError("❌ Falha ao deserializar PerfilAlteradoEvent");
            return;
        }

        _logger.LogInformation(
            "👑 Perfil alterado | UsuarioId: {UsuarioId} | {PerfilAntigo} → {PerfilNovo}",
            evento.UsuarioId, evento.PerfilAntigo, evento.PerfilNovo
        );

        using var scope = _serviceScopeFactory.CreateScope();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

        await emailService.EnviarEmailAlteracaoPerfilAsync(evento.Email, evento.PerfilAntigo, evento.PerfilNovo, evento.UsuarioId);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("🛑 UsersEventsConsumer parando...");

        _channel?.Close();
        _connection?.Close();

        await base.StopAsync(cancellationToken);
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }
}
