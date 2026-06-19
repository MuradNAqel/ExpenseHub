namespace EventBus.RabbitMQ;

public sealed class RabbitMqOptions
{
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string ExchangeName { get; set; } = "expensehub.events";
    public int RetryDelaySeconds { get; set; } = 5;
    public int ConnectionTimeoutSeconds { get; set; } = 5;
}
