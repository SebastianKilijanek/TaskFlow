using TaskFlow.Domain.Enums;

namespace TaskFlow.Domain.Entities
{
    public class User
    {
        public User() { }

        public User(Guid id, string email, string userName, string passwordHash, UserRole role)
        {
            Id = id;
            Email = email;
            UserName = userName;
            PasswordHash = passwordHash;
            Role = role;
        }

        public required Guid Id { get; set; }
        public required string Email { get; set; }
        public required string UserName { get; set; }
        public required string PasswordHash { get; set; }
        public UserRole Role { get; set; }

        public virtual ICollection<UserBoard> UserBoards { get; set; } = new List<UserBoard>();
        public virtual ICollection<TaskItem> AssignedTasks { get; set; } = new List<TaskItem>();
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}