namespace MqttOfThings;

public class User
{
    public string UserName { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;

    public string ClientId { get; init; } = string.Empty;
}

public class Config
{
    public List<User> Users = null!;
}