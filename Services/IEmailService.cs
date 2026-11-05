namespace FcgNotificationsApi.Services;

/// <summary>
/// Interface para serviço de envio de emails
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Envia email de boas-vindas para novo usuário
    /// </summary>
    Task EnviarEmailBoasVindasAsync(string email, string nome, Guid usuarioId);

    /// <summary>
    /// Envia email de confirmação após ativação da conta
    /// </summary>
    Task EnviarEmailConfirmacaoAtivacaoAsync(string email, Guid usuarioId);

    /// <summary>
    /// Envia email de notificação de alteração de senha
    /// </summary>
    Task EnviarEmailAlteracaoSenhaAsync(string email, Guid usuarioId);

    /// <summary>
    /// Envia email de notificação de alteração de perfil
    /// </summary>
    Task EnviarEmailAlteracaoPerfilAsync(string email, string perfilAntigo, string perfilNovo, Guid usuarioId);
}
