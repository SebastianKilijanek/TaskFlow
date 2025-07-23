namespace TaskFlow.Application.Boards.DTO
{
    public class BoardDTO(Guid boardId, string boardName, bool boardIsPublic)
    {
        public Guid Id { get; set; } = boardId;
        public string Name { get; set; } = boardName;
        public bool IsPublic { get; set; } = boardIsPublic;
    }
}