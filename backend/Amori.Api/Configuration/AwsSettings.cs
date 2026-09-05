namespace Amori.Api.Configuration;

public sealed class AwsSettings
{
    public const string SectionName = "Aws";

    public string Region { get; init; } = string.Empty;
    public string S3BucketName { get; init; } = string.Empty;
    public string AccessKeyId { get; init; } = string.Empty;
    public string SecretAccessKey { get; init; } = string.Empty;
}
