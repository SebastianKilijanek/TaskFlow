namespace TaskFlow.Application.Auth.DTO;

public record AuthResultDTO(string AccessToken, string RefreshToken, string UserName, string Email, string Role);