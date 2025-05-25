namespace FarmMetricsAPI.Models.Postgres
{
    public class Metric
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public double MinValue { get; set; }
        public double MaxValue { get; set; }

        public ICollection<SettleMetricDevice> SettleMetricDevices { get; set; }
            = new List<SettleMetricDevice>();
    }
}
