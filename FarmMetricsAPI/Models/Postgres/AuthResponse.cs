namespace FarmMetricsAPI.Models.Postgres
{
    public class AuthResponse
    {
        public string Token { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int UserId { get; set; }

    }
}
