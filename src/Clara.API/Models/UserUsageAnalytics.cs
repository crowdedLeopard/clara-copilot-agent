using System.Text.Json.Serialization;

namespace Clara.API.Models;

public class UserUsageAnalytics
{
    [JsonPropertyName("userId")]
    public required string UserId { get; set; }
    
    [JsonPropertyName("userDisplayName")]
    public required string UserDisplayName { get; set; }
    
    [JsonPropertyName("userPrincipalName")]
    public string? UserPrincipalName { get; set; }
    
    [JsonPropertyName("department")]
    public string? Department { get; set; }
    
    [JsonPropertyName("totalCopilotActions")]
    public int TotalCopilotActions { get; set; }
    
    [JsonPropertyName("copilotApplicationUsage")]
    public List<CopilotAppUsage> CopilotApplicationUsage { get; set; } = new();
    
    [JsonPropertyName("usageByTimeRange")]
    public UsageTimeRangeAnalytics UsageByTimeRange { get; set; } = new();
    
    [JsonPropertyName("productivityMetrics")]
    public ProductivityMetrics ProductivityMetrics { get; set; } = new();
    
    [JsonPropertyName("lastActivityDate")]
    public DateTime? LastActivityDate { get; set; }
    
    [JsonPropertyName("isActiveUser")]
    public bool IsActiveUser { get; set; }
    
    [JsonPropertyName("daysInactive")]
    public int? DaysInactive { get; set; }
}

public class CopilotAppUsage
{
    [JsonPropertyName("applicationName")]
    public required string ApplicationName { get; set; }
    
    [JsonPropertyName("lastActivityDate")]
    public DateTime? LastActivityDate { get; set; }
    
    [JsonPropertyName("actionsCount")]
    public int ActionsCount { get; set; }
    
    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; }
    
    [JsonPropertyName("daysSinceLastUse")]
    public int? DaysSinceLastUse { get; set; }
}

public class UsageTimeRangeAnalytics
{
    [JsonPropertyName("last7Days")]
    public int Last7Days { get; set; }
    
    [JsonPropertyName("last30Days")]
    public int Last30Days { get; set; }
    
    [JsonPropertyName("last90Days")]
    public int Last90Days { get; set; }
    
    [JsonPropertyName("currentMonth")]
    public int CurrentMonth { get; set; }
    
    [JsonPropertyName("previousMonth")]
    public int PreviousMonth { get; set; }
}

public class ProductivityMetrics
{
    [JsonPropertyName("engagementScore")]
    public double EngagementScore { get; set; }
    
    [JsonPropertyName("diversityScore")]
    public double DiversityScore { get; set; }
    
    [JsonPropertyName("consistencyScore")]
    public double ConsistencyScore { get; set; }
    
    [JsonPropertyName("trendDirection")]
    public string TrendDirection { get; set; } = "stable";
}

public class UsageSummaryReport
{
    [JsonPropertyName("reportGeneratedAt")]
    public DateTime ReportGeneratedAt { get; set; }
    
    [JsonPropertyName("dateRange")]
    public DateRangeInfo DateRange { get; set; } = new();
    
    [JsonPropertyName("totalUsers")]
    public int TotalUsers { get; set; }
    
    [JsonPropertyName("activeUsers")]
    public int ActiveUsers { get; set; }
    
    [JsonPropertyName("inactiveUsers")]
    public int InactiveUsers { get; set; }
    
    [JsonPropertyName("totalCopilotActions")]
    public int TotalCopilotActions { get; set; }
    
    [JsonPropertyName("averageActionsPerUser")]
    public double AverageActionsPerUser { get; set; }
    
    [JsonPropertyName("mostUsedApplications")]
    public List<ApplicationUsageStats> MostUsedApplications { get; set; } = new();
    
    [JsonPropertyName("departmentBreakdown")]
    public List<DepartmentUsageStats> DepartmentBreakdown { get; set; } = new();
    
    [JsonPropertyName("usageTrends")]
    public UsageTrends UsageTrends { get; set; } = new();
}

public class DateRangeInfo
{
    [JsonPropertyName("startDate")]
    public DateTime StartDate { get; set; }
    
    [JsonPropertyName("endDate")]
    public DateTime EndDate { get; set; }
    
    [JsonPropertyName("days")]
    public int Days { get; set; }
}

public class ApplicationUsageStats
{
    [JsonPropertyName("applicationName")]
    public required string ApplicationName { get; set; }
    
    [JsonPropertyName("totalActions")]
    public int TotalActions { get; set; }
    
    [JsonPropertyName("uniqueUsers")]
    public int UniqueUsers { get; set; }
    
    [JsonPropertyName("averageActionsPerUser")]
    public double AverageActionsPerUser { get; set; }
    
    [JsonPropertyName("adoptionRate")]
    public double AdoptionRate { get; set; }
}

public class DepartmentUsageStats
{
    [JsonPropertyName("departmentName")]
    public required string DepartmentName { get; set; }
    
    [JsonPropertyName("totalUsers")]
    public int TotalUsers { get; set; }
    
    [JsonPropertyName("activeUsers")]
    public int ActiveUsers { get; set; }
    
    [JsonPropertyName("adoptionRate")]
    public double AdoptionRate { get; set; }
    
    [JsonPropertyName("totalActions")]
    public int TotalActions { get; set; }
    
    [JsonPropertyName("averageActionsPerUser")]
    public double AverageActionsPerUser { get; set; }
}

public class UsageTrends
{
    [JsonPropertyName("weekOverWeekGrowth")]
    public double WeekOverWeekGrowth { get; set; }
    
    [JsonPropertyName("monthOverMonthGrowth")]
    public double MonthOverMonthGrowth { get; set; }
    
    [JsonPropertyName("trendDirection")]
    public string TrendDirection { get; set; } = "stable";
    
    [JsonPropertyName("peakUsageDay")]
    public string? PeakUsageDay { get; set; }
    
    [JsonPropertyName("averageDailyActions")]
    public double AverageDailyActions { get; set; }
}
