namespace FarmMetricsAPI.Models.Postgres
{
    public class User
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? PasswordHash { get; set; }
        public string? Phone { get; set; }

        public int? SettlementId { get; set; } //FK
        public Settlement? Settlement { get; set; }

        public int RoleId { get; set; }
        public Role? Role { get; set; }

    }
}
