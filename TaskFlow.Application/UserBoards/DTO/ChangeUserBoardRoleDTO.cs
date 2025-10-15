using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.UserBoards.DTO;

public class ChangeUserBoardRoleDTO
{
    public required BoardRole NewRole { get; set; }
}