using System.ComponentModel.DataAnnotations;

namespace APIDoctorCheckUp.Application.DTOs;

public record LoginDto(
    [Required] string Username,
    [Required] string Password
);

public record TokenDto(
    string AccessToken,
    DateTime ExpiresAt
);
