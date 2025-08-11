namespace TaskFlow.Application.Columns.DTO;

public class ColumnDTO
{
    public ColumnDTO() { }

    public ColumnDTO(Guid id, string name, int position, Guid boardId)
    {
        Id = id;
        Name = name;
        Position = position;
        BoardId = boardId;
    }

    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public int Position { get; set; }
    public required Guid BoardId { get; set; }
}