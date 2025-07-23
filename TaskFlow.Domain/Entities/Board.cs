namespace TaskFlow.Domain.Entities
{
    public class Board
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsPublic { get; set; }

        public ICollection<Column> Columns { get; set; } = new List<Column>();
        public ICollection<UserBoard> UserBoards { get; set; } = new List<UserBoard>();
    }

}