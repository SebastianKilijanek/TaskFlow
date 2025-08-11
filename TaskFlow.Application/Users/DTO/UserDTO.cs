namespace TaskFlow.Application.Users.DTO
{
    public class UserDTO
    {
        public UserDTO() { }

        public UserDTO(Guid id, string email, string userName, string role)
        {
            Id = id;
            Email = email;
            UserName = userName;
            Role = role;
        }

        public required Guid Id { get; set; }
        public required string Email { get; set; }
        public required string UserName { get; set; }
        public required string Role { get; set; }
    }
}