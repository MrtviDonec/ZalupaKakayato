namespace NetworkWorm.Server.Services
{
    public class AuthService
    {
        // Временное решение - без JWT
        public string GenerateToken(int userId, string username, string role)
        {
            // Возвращаем пустую строку, так как JWT отключен
            return string.Empty;
        }
    }
}
