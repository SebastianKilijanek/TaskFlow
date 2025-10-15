namespace TaskFlow.Application.Users.DTO;

public class ChangeUserPasswordDTO
{
    public required string CurrentPassword { get; set; }
    public required string NewPassword { get; set; }
}