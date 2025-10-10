namespace TaskFlow.Application.Boards.DTO;

public class CreateBoardDTO
{
    public required string Name { get; set; }
    public bool IsPublic { get; set; }
}