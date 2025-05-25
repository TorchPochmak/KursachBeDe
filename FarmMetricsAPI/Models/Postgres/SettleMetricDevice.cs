namespace FarmMetricsAPI.Models.Postgres
{
    public class SettleMetricDevice
    {
        public int Id { get; set; }
        public int MetricId { get; set; } //FK
        public Metric? Metric { get; set; }
        public int SettlementId { get; set; } //FK
        public Settlement? Settlement { get; set; }
        public DateTime RegisteredAt { get; set; }

        public ICollection<MetricData> MetricData { get; set; }
            = new List<MetricData>();
    }
}
