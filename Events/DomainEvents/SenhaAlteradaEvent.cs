namespace FcgNotificationsApi.Events.DomainEvents;

/// <summary>
/// Evento recebido quando uma senha é alterada na Users API
/// </summary>
public class SenhaAlteradaEvent : IDomainEvent
{
    public Guid EventId { get; set; }
    public DateTime OcorridoEm { get; set; }
    public string EventType { get; set; } = "SenhaAlterada";

    public Guid UsuarioId { get; set; }
    public string Email { get; set; } = string.Empty;
    public DateTime AlteradaEm { get; set; }
}
