using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.UserBoards.DTO;

public class AddUserToBoardDTO
{
    public required string UserEmail { get; set; }
    public required BoardRole BoardRole { get; set; }
}