namespace Amori.Api.Configuration;

public sealed class NotificationSettings
{
    public const string SectionName = "Notifications";

    public string PushNotificationKey { get; init; } = string.Empty;
}
