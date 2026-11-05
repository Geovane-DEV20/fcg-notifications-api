namespace FcgNotificationsApi.Events.DomainEvents;

/// <summary>
/// Evento recebido quando um perfil é alterado na Users API
/// </summary>
public class PerfilAlteradoEvent : IDomainEvent
{
    public Guid EventId { get; set; }
    public DateTime OcorridoEm { get; set; }
    public string EventType { get; set; } = "PerfilAlterado";

    public Guid UsuarioId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PerfilAntigo { get; set; } = string.Empty;
    public string PerfilNovo { get; set; } = string.Empty;
    public string AlteradoPor { get; set; } = string.Empty;
}
