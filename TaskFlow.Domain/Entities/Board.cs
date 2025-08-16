namespace TaskFlow.Domain.Entities;

public class Board
{
    public Board() { }

    public Board(Guid id, string name, bool isPublic)
    {
        Id = id;
        Name = name;
        IsPublic = isPublic;
    }

    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public bool IsPublic { get; set; }

    public virtual ICollection<Column> Columns { get; set; } = new List<Column>();
    public virtual ICollection<UserBoard> UserBoards { get; set; } = new List<UserBoard>();
}