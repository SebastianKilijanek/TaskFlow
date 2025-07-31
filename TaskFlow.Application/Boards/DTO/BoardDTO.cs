namespace TaskFlow.Application.Boards.DTO;

public class BoardDTO
{
    public BoardDTO() { }

    public BoardDTO(Guid boardId, string boardName, bool boardIsPublic)
    {
        Id = boardId;
        Name = boardName;
        IsPublic = boardIsPublic;
    }

    public required  Guid Id { get; set; }
    public required string Name { get; set; }
    public bool IsPublic { get; set; }
}
