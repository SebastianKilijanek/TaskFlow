namespace TaskFlow.Domain.Entities;

public class Column
{
    public Column() { }

    public Column(Guid id, string name, int position, Guid boardId)
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
    public virtual Board Board { get; set; } = null!;
    public virtual ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}