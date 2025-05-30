using System.ComponentModel.DataAnnotations;

namespace FarmMetricsAPI.Models.Mongo;

public class CreateFarmRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public int SettlementId { get; set; }
} 