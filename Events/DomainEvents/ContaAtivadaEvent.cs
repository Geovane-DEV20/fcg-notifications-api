namespace FcgNotificationsApi.Events.DomainEvents;

/// <summary>
/// Evento recebido quando uma conta é ativada na Users API
/// </summary>
public class ContaAtivadaEvent : IDomainEvent
{
    public Guid EventId { get; set; }
    public DateTime OcorridoEm { get; set; }
    public string EventType { get; set; } = "ContaAtivada";

    public Guid UsuarioId { get; set; }
    public string Email { get; set; } = string.Empty;
    public DateTime AtivadaEm { get; set; }
}
