namespace NetworkWorm.Server.Models
{
    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class LoginResponse
    {
        public bool Success { get; set; }
        public UserInfo User { get; set; }
        public string Token { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class UserInfo
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
    }

    public class SectionDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int Order { get; set; }
    }

    public class TheoryPartDto
    {
        public int Id { get; set; }
        public int SectionId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public int Order { get; set; }
    }
}