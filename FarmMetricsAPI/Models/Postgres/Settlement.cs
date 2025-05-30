namespace FarmMetricsAPI.Models.Postgres
{
    public class Settlement
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public int Population { get; set; }

        public ICollection<User> Users { get; set; }
            = new List<User>();
        public ICollection<SettleMetricDevice> SettleMetricDevices { get; set; }
            = new List<SettleMetricDevice>();
    }
}
