namespace TaskFlow.Application.Users.DTO;

public class UpdateUserDTO
{
    public required string Email { get; set; }
    public required string UserName { get; set; }
    public required string Role { get; set; }
}