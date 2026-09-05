namespace Amori.Api.Common.Exceptions;

public sealed class NotFoundException(string message) : Exception(message)
{
    public NotFoundException(string entityName, object key)
        : this($"{entityName} with id '{key}' was not found.") { }
}
