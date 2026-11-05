namespace FcgNotificationsApi.Models;

public class Notification
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Type { get; set; } = string.Empty; // "Welcome", "PurchaseConfirmation"
    public string Email { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
}

public class UserCreatedEvent
{
    public Guid UserId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

public class PaymentProcessedEvent
{
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
    public Guid GameId { get; set; }
    public decimal Price { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
