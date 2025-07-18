using TaskFlow.Domain.Enums;

namespace TaskFlow.Domain.Entities
{
    public class UserBoard
    {
        public Guid UserId { get; set; }
        public User User { get; set; }
        public Guid BoardId { get; set; }
        public Board Board { get; set; }
        public BoardRole BoardRole { get; set; }
        public DateTime JoinedAt { get; set; }
    }
}