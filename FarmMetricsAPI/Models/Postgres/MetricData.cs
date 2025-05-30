namespace FarmMetricsAPI.Models.Postgres
{
    public class MetricData
    {
        public int Id { get; set; }
        public DateTime RegisteredAt { get; set; }
        public int SettleMetricDeviceId { get; set; } //FK
        public SettleMetricDevice? Device { get; set; }
        public double MetricValue { get; set; }
    }
}
