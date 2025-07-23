namespace TaskFlow.Application.Boards.DTO
{
    public class BoardDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public bool IsPublic { get; set; }
    }
}