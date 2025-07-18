namespace TaskFlow.Domain.Entities
{
    public class Board
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public bool IsPublic { get; set; }
        public ICollection<Column> Columns { get; set; }
        public ICollection<UserBoard> UserBoards { get; set; } // relation N:N
    }
}