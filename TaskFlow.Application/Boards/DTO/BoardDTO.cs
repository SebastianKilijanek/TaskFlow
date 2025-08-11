namespace TaskFlow.Application.Boards.DTO;

public class BoardDTO
{
    public BoardDTO() { }

    public BoardDTO(Guid id, string name, bool isPublic)
    {
        Id = id;
        Name = name;
        IsPublic = isPublic;
    }

    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public bool IsPublic { get; set; }
}