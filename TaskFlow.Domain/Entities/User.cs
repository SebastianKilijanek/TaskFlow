using TaskFlow.Domain.Enums;

namespace TaskFlow.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string PasswordHash { get; set; }
        public ICollection<UserBoard> UserBoards { get; set; }
        public ICollection<TaskItem> AssignedTasks { get; set; }
        public UserRole Role { get; set; }
    }
}