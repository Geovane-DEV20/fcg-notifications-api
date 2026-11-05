namespace FcgNotificationsApi.Services;

public interface IRabbitMqService
{
    void StartConsuming(string routingKey, Action<string> messageHandler);
}
