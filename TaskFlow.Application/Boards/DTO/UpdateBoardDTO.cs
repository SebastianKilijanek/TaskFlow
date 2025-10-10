namespace TaskFlow.Application.Boards.DTO;

public class UpdateBoardDTO
{
    public required string Name { get; set; }
    public bool IsPublic { get; set; }
}