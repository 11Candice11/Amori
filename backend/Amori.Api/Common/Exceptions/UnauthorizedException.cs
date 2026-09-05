namespace Amori.Api.Common.Exceptions;

public sealed class UnauthorizedException(string message = "You are not authorized to perform this action.")
    : Exception(message);
