namespace FcgNotificationsApi.Events.DomainEvents;

/// <summary>
/// Evento recebido quando um usuário é criado na Users API
/// </summary>
public class UsuarioCriadoEvent : IDomainEvent
{
    public Guid EventId { get; set; }
    public DateTime OcorridoEm { get; set; }
    public string EventType { get; set; } = "UsuarioCriado";

    public Guid UsuarioId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Perfil { get; set; } = string.Empty;
    public DateTime CriadoEm { get; set; }
}
