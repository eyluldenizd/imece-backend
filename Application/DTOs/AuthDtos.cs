namespace Application.DTOs;

public sealed record LoginRequestDto(string Username, string Password);

public sealed record LoginResponseDto(
    string AccessToken,
    DateTime ExpiresAt,
    string TokenType,
    CurrentUserResponse User);
