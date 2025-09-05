using System.Text.Json.Serialization;

namespace Clara.API.Models;

public class LicenseCountsDto
{
    [JsonPropertyName("totalLicenses")]
    public int TotalLicenses { get; set; }
    
    [JsonPropertyName("usedLicenses")]
    public int AssignedLicenses { get; set; }
    
    [JsonPropertyName("availableLicenses")]
    public int AvailableLicenses { get; set; }
}
