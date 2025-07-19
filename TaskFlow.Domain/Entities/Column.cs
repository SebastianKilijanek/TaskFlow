namespace TaskFlow.Domain.Entities
{
    public class Column
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int Order { get; set; }
        public Guid BoardId { get; set; }
        public Board Board { get; set; }
        public ICollection<TaskItem> Tasks { get; set; }
        public int Position { get; set; }
    }
}