namespace Clara.API.Interfaces;

using Clara.API.Models;

public interface ICopilotUsageService
{
    Task<IEnumerable<M365CopilotUsageReport>> GetInactiveUsersAsync(int? days = null);
    
    // Enhanced analytics methods
    Task<UserUsageAnalytics> GetUserUsageAnalyticsAsync(string userId, int days = 30);
    Task<IEnumerable<UserUsageAnalytics>> GetAllUsersAnalyticsAsync(int days = 30);
    Task<UsageSummaryReport> GetUsageSummaryReportAsync(int days = 30);
    Task<IEnumerable<UserUsageAnalytics>> GetTopUsersAsync(int topCount = 10, int days = 30);
    Task<IEnumerable<UserUsageAnalytics>> GetUsersByDepartmentAsync(string department, int days = 30);
}
