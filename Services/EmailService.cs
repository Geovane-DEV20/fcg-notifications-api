namespace FcgNotificationsApi.Services;

/// <summary>
/// Implementação simulada do serviço de envio de emails
/// Em produção, integrar com SendGrid, AWS SES, Azure Communication Services, etc.
/// </summary>
public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public async Task EnviarEmailBoasVindasAsync(string email, string nome, Guid usuarioId)
    {
        _logger.LogInformation(
            "📧 [EMAIL] Enviando email de boas-vindas | Para: {Email} | Nome: {Nome} | UsuarioId: {UsuarioId}",
            email, nome, usuarioId
        );

        // Simula tempo de processamento
        await Task.Delay(500);

        // Simulação do conteúdo do email
        var assunto = "Bem-vindo à FIAP Cloud Games!";
        var corpo = $@"
Olá {nome}!

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
";

        _logger.LogInformation(
            "✅ [EMAIL ENVIADO] Assunto: {Assunto} | Para: {Email}",
            assunto, email
        );

        // Em produção:
        // await _sendGridClient.SendEmailAsync(email, assunto, corpo);
        // ou
        // await _awsSesClient.SendEmailAsync(...);
    }

    public async Task EnviarEmailConfirmacaoAtivacaoAsync(string email, Guid usuarioId)
    {
        _logger.LogInformation(
            "📧 [EMAIL] Enviando confirmação de ativação | Para: {Email} | UsuarioId: {UsuarioId}",
            email, usuarioId
        );

        await Task.Delay(500);

        var assunto = "Conta ativada com sucesso! 🎉";
        var corpo = $@"
Parabéns!

Sua conta FIAP Cloud Games foi ativada com sucesso! 🎊

Agora você já pode:
✅ Navegar pelo catálogo de jogos
✅ Adicionar jogos à sua biblioteca
✅ Gerenciar seus dados

Acesse agora: https://fiapcloudgames.com/login

Bons jogos!
Equipe FIAP Cloud Games
";

        _logger.LogInformation(
            "✅ [EMAIL ENVIADO] Assunto: {Assunto} | Para: {Email}",
            assunto, email
        );
    }

    public async Task EnviarEmailAlteracaoSenhaAsync(string email, Guid usuarioId)
    {
        _logger.LogInformation(
            "📧 [EMAIL] Enviando notificação de alteração de senha | Para: {Email} | UsuarioId: {UsuarioId}",
            email, usuarioId
        );

        await Task.Delay(500);

        var assunto = "Sua senha foi alterada";
        var corpo = $@"
Olá!

Informamos que sua senha foi alterada recentemente.

Data/Hora: {DateTime.UtcNow:dd/MM/yyyy HH:mm} UTC

Se você não realizou esta alteração, entre em contato 
imediatamente com nosso suporte em seguranca@fiapcloudgames.com

Equipe FIAP Cloud Games
";

        _logger.LogInformation(
            "✅ [EMAIL ENVIADO] Assunto: {Assunto} | Para: {Email}",
            assunto, email
        );
    }

    public async Task EnviarEmailAlteracaoPerfilAsync(string email, string perfilAntigo, string perfilNovo, Guid usuarioId)
    {
        _logger.LogInformation(
            "📧 [EMAIL] Enviando notificação de alteração de perfil | Para: {Email} | {PerfilAntigo} → {PerfilNovo} | UsuarioId: {UsuarioId}",
            email, perfilAntigo, perfilNovo, usuarioId
        );

        await Task.Delay(500);

        var assunto = "Seu perfil foi alterado";
        var corpo = $@"
Olá!

Informamos que seu perfil de acesso foi alterado.

Perfil anterior: {perfilAntigo}
Novo perfil: {perfilNovo}
Data/Hora: {DateTime.UtcNow:dd/MM/yyyy HH:mm} UTC

Esta alteração foi realizada por um administrador do sistema.

Equipe FIAP Cloud Games
";

        _logger.LogInformation(
            "✅ [EMAIL ENVIADO] Assunto: {Assunto} | Para: {Email}",
            assunto, email
        );
    }
}
