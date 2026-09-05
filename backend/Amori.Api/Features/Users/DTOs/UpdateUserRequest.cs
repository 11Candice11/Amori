using System.ComponentModel.DataAnnotations;

namespace Amori.Api.Features.Users.DTOs;

public sealed class UpdateUserRequest
{
    [MaxLength(100)]
    public string? Name { get; set; }

    [MaxLength(500)]
    public string? AvatarUrl { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    [MaxLength(20)]
    public string? PhoneNumber { get; set; }
}
