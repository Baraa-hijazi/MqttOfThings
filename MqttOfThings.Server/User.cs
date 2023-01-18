namespace MqttOfThings;

public class User
{
    public string UserName { get; set; } = string.Empty;

    public string ClientId { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public TopicTuple SubscriptionTopicLists { get; set; } = new();

    public TopicTuple PublishTopicLists { get; set; } = new();
}

public class TopicTuple
{
    public List<string> WhitelistTopics { get; set; } = new();

    public List<string> BlacklistTopics { get; set; } = new();
}

public class Config
{
    public List<User> Users { get; set; } = new();
}