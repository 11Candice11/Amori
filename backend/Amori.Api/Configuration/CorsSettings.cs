namespace Amori.Api.Configuration;

public sealed class CorsSettings
{
    public const string SectionName = "Cors";
    public const string PolicyName = "AmoriCorsPolicy";

    public string[] AllowedOrigins { get; init; } = [];
}
