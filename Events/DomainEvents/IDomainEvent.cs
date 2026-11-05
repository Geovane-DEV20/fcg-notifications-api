namespace FcgNotificationsApi.Events.DomainEvents;

/// <summary>
/// Interface base para eventos de domínio recebidos via RabbitMQ
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// Identificador único do evento
    /// </summary>
    Guid EventId { get; }

    /// <summary>
    /// Timestamp de quando o evento ocorreu
    /// </summary>
    DateTime OcorridoEm { get; }

    /// <summary>
    /// Tipo do evento
    /// </summary>
    string EventType { get; }
}
