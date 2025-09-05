using Azure.Identity;
using Clara.API.Models;
using Microsoft.Graph;
using System.Net.Http.Headers;
using Clara.API.Interfaces;
using Newtonsoft.Json.Linq;

namespace Clara.API.Classes;

public class CopilotUsageService : ICopilotUsageService
{
    private readonly GraphServiceClient _graphClient;
    private readonly ClientSecretCredential _credential;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _copilotSkuId;
    private readonly string _m365CopilotDashboardUrl;

    public CopilotUsageService(
        GraphServiceClient graphClient,
        ClientSecretCredential credential,
        IHttpClientFactory httpClientFactory,
        IConfiguration config)
    {
        _graphClient = graphClient;
        _credential = credential;
        _httpClientFactory = httpClientFactory;
        _copilotSkuId = config["CopilotSkuId"]!;
        _m365CopilotDashboardUrl = config["M365CopilotDashboardUrl"]!;
    }

    public async Task<IEnumerable<M365CopilotUsageReport>> GetInactiveUsersAsync(int? days = null)
    {
         // Fetch users with Copilot licenses
        var users = await _graphClient.Users
            .GetAsync(requestConfig =>
            {
                requestConfig.QueryParameters.Filter = $"assignedLicenses/any(x:x/skuId eq {_copilotSkuId})";
                requestConfig.QueryParameters.Select = new[] { "id", "userPrincipalName"};
            });

        // Fetch Copilot usage report
        var usageList = await GetCopilotUsageReport();
        
        // Validate inputs
        if (users?.Value == null || usageList == null)
        {
            return Enumerable.Empty<M365CopilotUsageReport>();
        }

        
        // Build a HashSet of userPrincipalNames from the users list for fast lookup
        var userPrincipalNames = new HashSet<string>(
            users.Value.Select(u => u.UserPrincipalName!),
            StringComparer.OrdinalIgnoreCase
        );

        // Filter usageList to only those present in users list
        var filteredUsageList = usageList
            .Where(report => userPrincipalNames.Contains(report.UserPrincipalName!))
            .ToList();


        var joined = from usage in usageList
             join user in users.Value
             on usage.UserPrincipalName!.ToLowerInvariant() equals user.UserPrincipalName!.ToLowerInvariant()
             select new M365CopilotUsageReport
             {
                 UserId = user.Id!,
                 UserDisplayName = usage.UserDisplayName,
                 UserPrincipalName = usage.UserPrincipalName,
                 UserDepartment = usage.UserDepartment,
                 LastActivityDate = usage.LastActivityDate,
                 CopilotChatLastActivityDate = usage.CopilotChatLastActivityDate,
                 MicrosoftTeamsCopilotLastActivityDate = usage.MicrosoftTeamsCopilotLastActivityDate,
                 WordCopilotLastActivityDate = usage.WordCopilotLastActivityDate,
                 ExcelCopilotLastActivityDate = usage.ExcelCopilotLastActivityDate,
                 PowerPointCopilotLastActivityDate = usage.PowerPointCopilotLastActivityDate,
                 OutlookCopilotLastActivityDate = usage.OutlookCopilotLastActivityDate,
                 OneNoteCopilotLastActivityDate = usage.OneNoteCopilotLastActivityDate,
                 LoopCopilotLastActivityDate = usage.LoopCopilotLastActivityDate
             };



        // Optionally filter by days
        if (days.HasValue)
        {
            var filteredList = joined
                .Where(report =>
                            report.LastActivityDate == null ||
                            (
                                report.LastActivityDate is DateTime lastActivity &&
                                (DateTime.UtcNow.Subtract(lastActivity)).TotalDays >= days.Value
                            )
                       );

            return filteredList;
        }
    
        return joined;
    }

    public async Task<UserUsageAnalytics> GetUserUsageAnalyticsAsync(string userId, int days = 30)
    {
        var allUsersAnalytics = await GetAllUsersAnalyticsAsync(days);
        var userAnalytics = allUsersAnalytics.FirstOrDefault(u => u.UserId == userId);
        
        if (userAnalytics == null)
        {
            throw new ArgumentException($"User with ID {userId} not found or has no Copilot license");
        }
        
        return userAnalytics;
    }

    public async Task<IEnumerable<UserUsageAnalytics>> GetAllUsersAnalyticsAsync(int days = 30)
    {
        // Get the base usage report
        var usageReports = await GetInactiveUsersAsync(null);
        var analytics = new List<UserUsageAnalytics>();

        foreach (var report in usageReports)
        {
            var userAnalytics = await BuildUserAnalytics(report, days);
            analytics.Add(userAnalytics);
        }

        return analytics;
    }

    public async Task<UsageSummaryReport> GetUsageSummaryReportAsync(int days = 30)
    {
        var allUsersAnalytics = await GetAllUsersAnalyticsAsync(days);
        var usersList = allUsersAnalytics.ToList();

        var summary = new UsageSummaryReport
        {
            ReportGeneratedAt = DateTime.UtcNow,
            DateRange = new DateRangeInfo
            {
                StartDate = DateTime.UtcNow.AddDays(-days),
                EndDate = DateTime.UtcNow,
                Days = days
            },
            TotalUsers = usersList.Count,
            ActiveUsers = usersList.Count(u => u.IsActiveUser),
            InactiveUsers = usersList.Count(u => !u.IsActiveUser),
            TotalCopilotActions = usersList.Sum(u => u.TotalCopilotActions),
            AverageActionsPerUser = usersList.Count > 0 ? usersList.Average(u => u.TotalCopilotActions) : 0
        };

        // Calculate application usage stats
        summary.MostUsedApplications = CalculateApplicationStats(usersList);
        
        // Calculate department breakdown
        summary.DepartmentBreakdown = CalculateDepartmentStats(usersList);
        
        // Calculate usage trends
        summary.UsageTrends = await CalculateUsageTrends(usersList, days);

        return summary;
    }

    public async Task<IEnumerable<UserUsageAnalytics>> GetTopUsersAsync(int topCount = 10, int days = 30)
    {
        var allUsersAnalytics = await GetAllUsersAnalyticsAsync(days);
        
        return allUsersAnalytics
            .OrderByDescending(u => u.TotalCopilotActions)
            .Take(topCount);
    }

    public async Task<IEnumerable<UserUsageAnalytics>> GetUsersByDepartmentAsync(string department, int days = 30)
    {
        var allUsersAnalytics = await GetAllUsersAnalyticsAsync(days);
        
        return allUsersAnalytics
            .Where(u => string.Equals(u.Department, department, StringComparison.OrdinalIgnoreCase));
    }

    // --- Private helper methods for analytics ---

    private Task<UserUsageAnalytics> BuildUserAnalytics(M365CopilotUsageReport report, int days)
    {
        var analytics = new UserUsageAnalytics
        {
            UserId = report.UserId,
            UserDisplayName = report.UserDisplayName,
            UserPrincipalName = report.UserPrincipalName,
            Department = report.UserDepartment,
            LastActivityDate = report.LastActivityDate
        };

        // Calculate days inactive
        if (report.LastActivityDate.HasValue)
        {
            analytics.DaysInactive = (int)(DateTime.UtcNow - report.LastActivityDate.Value).TotalDays;
            analytics.IsActiveUser = analytics.DaysInactive <= days;
        }
        else
        {
            analytics.DaysInactive = null;
            analytics.IsActiveUser = false;
        }

        // Build application usage
        analytics.CopilotApplicationUsage = BuildApplicationUsage(report);
        
        // Calculate total actions (simulated based on app usage)
        analytics.TotalCopilotActions = CalculateTotalActions(analytics.CopilotApplicationUsage);
        
        // Build time range analytics (simulated)
        analytics.UsageByTimeRange = BuildTimeRangeAnalytics(report, days);
        
        // Calculate productivity metrics
        analytics.ProductivityMetrics = CalculateProductivityMetrics(analytics);

        return Task.FromResult(analytics);
    }

    private List<CopilotAppUsage> BuildApplicationUsage(M365CopilotUsageReport report)
    {
        var apps = new List<CopilotAppUsage>();
        
        // Map each Copilot application
        var appMappings = new Dictionary<string, DateTime?>
        {
            ["Copilot Chat"] = report.CopilotChatLastActivityDate,
            ["Microsoft Teams"] = report.MicrosoftTeamsCopilotLastActivityDate,
            ["Word"] = report.WordCopilotLastActivityDate,
            ["Excel"] = report.ExcelCopilotLastActivityDate,
            ["PowerPoint"] = report.PowerPointCopilotLastActivityDate,
            ["Outlook"] = report.OutlookCopilotLastActivityDate,
            ["OneNote"] = report.OneNoteCopilotLastActivityDate,
            ["Loop"] = report.LoopCopilotLastActivityDate
        };

        foreach (var mapping in appMappings)
        {
            var app = new CopilotAppUsage
            {
                ApplicationName = mapping.Key,
                LastActivityDate = mapping.Value,
                IsActive = mapping.Value.HasValue && (DateTime.UtcNow - mapping.Value.Value).TotalDays <= 30
            };
            
            if (mapping.Value.HasValue)
            {
                app.DaysSinceLastUse = (int)(DateTime.UtcNow - mapping.Value.Value).TotalDays;
                // Simulate actions count based on recency
                app.ActionsCount = Math.Max(1, 50 - app.DaysSinceLastUse.Value);
            }
            else
            {
                app.DaysSinceLastUse = null;
                app.ActionsCount = 0;
            }
            
            apps.Add(app);
        }

        return apps;
    }

    private int CalculateTotalActions(List<CopilotAppUsage> appUsage)
    {
        return appUsage.Sum(app => app.ActionsCount);
    }

    private UsageTimeRangeAnalytics BuildTimeRangeAnalytics(M365CopilotUsageReport report, int days)
    {
        // This is a simplified simulation - in a real scenario, you'd have time-series data
        var totalActions = CalculateTotalActions(BuildApplicationUsage(report));
        var isActive = report.LastActivityDate.HasValue && 
                      (DateTime.UtcNow - report.LastActivityDate.Value).TotalDays <= days;

        if (!isActive)
        {
            return new UsageTimeRangeAnalytics();
        }

        // Distribute actions across time ranges (simulation)
        return new UsageTimeRangeAnalytics
        {
            Last7Days = (int)(totalActions * 0.3),
            Last30Days = (int)(totalActions * 0.7),
            Last90Days = totalActions,
            CurrentMonth = (int)(totalActions * 0.5),
            PreviousMonth = (int)(totalActions * 0.3)
        };
    }

    private ProductivityMetrics CalculateProductivityMetrics(UserUsageAnalytics analytics)
    {
        var activeApps = analytics.CopilotApplicationUsage.Count(app => app.IsActive);
        var totalApps = analytics.CopilotApplicationUsage.Count;
        
        // Calculate engagement score (0-100)
        var engagementScore = analytics.TotalCopilotActions > 0 ? 
            Math.Min(100, (analytics.TotalCopilotActions / 10.0) * 100) : 0;
        
        // Calculate diversity score (0-100) - how many different apps they use
        var diversityScore = totalApps > 0 ? (activeApps / (double)totalApps) * 100 : 0;
        
        // Calculate consistency score (0-100) - based on recent activity
        var consistencyScore = analytics.IsActiveUser ? 
            Math.Min(100, analytics.UsageByTimeRange.Last7Days * 10) : 0;
        
        var trendDirection = "stable";
        if (analytics.UsageByTimeRange.CurrentMonth > analytics.UsageByTimeRange.PreviousMonth * 1.1)
            trendDirection = "increasing";
        else if (analytics.UsageByTimeRange.CurrentMonth < analytics.UsageByTimeRange.PreviousMonth * 0.9)
            trendDirection = "decreasing";

        return new ProductivityMetrics
        {
            EngagementScore = Math.Round((double)engagementScore, 1),
            DiversityScore = Math.Round((double)diversityScore, 1),
            ConsistencyScore = Math.Round((double)consistencyScore, 1),
            TrendDirection = trendDirection
        };
    }

    private List<ApplicationUsageStats> CalculateApplicationStats(List<UserUsageAnalytics> usersList)
    {
        var appStats = new Dictionary<string, ApplicationUsageStats>();
        
        foreach (var user in usersList)
        {
            foreach (var app in user.CopilotApplicationUsage)
            {
                if (!appStats.ContainsKey(app.ApplicationName))
                {
                    appStats[app.ApplicationName] = new ApplicationUsageStats
                    {
                        ApplicationName = app.ApplicationName,
                        TotalActions = 0,
                        UniqueUsers = 0
                    };
                }
                
                appStats[app.ApplicationName].TotalActions += app.ActionsCount;
                if (app.IsActive)
                {
                    appStats[app.ApplicationName].UniqueUsers++;
                }
            }
        }
        
        // Calculate averages and adoption rates
        foreach (var stat in appStats.Values)
        {
            stat.AverageActionsPerUser = stat.UniqueUsers > 0 ? 
                stat.TotalActions / (double)stat.UniqueUsers : 0;
            stat.AdoptionRate = usersList.Count > 0 ? 
                (stat.UniqueUsers / (double)usersList.Count) * 100 : 0;
        }
        
        return appStats.Values
            .OrderByDescending(s => s.TotalActions)
            .ToList();
    }

    private List<DepartmentUsageStats> CalculateDepartmentStats(List<UserUsageAnalytics> usersList)
    {
        var deptStats = usersList
            .GroupBy(u => u.Department ?? "Unknown")
            .Select(g => new DepartmentUsageStats
            {
                DepartmentName = g.Key,
                TotalUsers = g.Count(),
                ActiveUsers = g.Count(u => u.IsActiveUser),
                TotalActions = g.Sum(u => u.TotalCopilotActions),
                AdoptionRate = g.Count() > 0 ? (g.Count(u => u.IsActiveUser) / (double)g.Count()) * 100 : 0,
                AverageActionsPerUser = g.Count() > 0 ? g.Average(u => u.TotalCopilotActions) : 0
            })
            .OrderByDescending(d => d.TotalActions)
            .ToList();
            
        return deptStats;
    }

    private Task<UsageTrends> CalculateUsageTrends(List<UserUsageAnalytics> usersList, int days)
    {
        // This is simplified - in reality you'd compare with historical data
        var totalActions = usersList.Sum(u => u.TotalCopilotActions);
        var activeUsers = usersList.Count(u => u.IsActiveUser);
        
        return Task.FromResult(new UsageTrends
        {
            WeekOverWeekGrowth = 5.2, // Simulated
            MonthOverMonthGrowth = 12.8, // Simulated
            TrendDirection = "increasing",
            PeakUsageDay = "Wednesday",
            AverageDailyActions = totalActions / (double)days
        });
    }

  

    // --- Helper methods ---

   private async Task<List<M365CopilotUsageReport>> GetCopilotUsageReport()
{
    var token = await _credential.GetTokenAsync(
        new Azure.Core.TokenRequestContext(new[] { "https://graph.microsoft.com/.default" }));

    var httpClient = _httpClientFactory.CreateClient();
    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);

    var usageList = new List<M365CopilotUsageReport>();
    string requestUrl = _m365CopilotDashboardUrl;

    while (!string.IsNullOrEmpty(requestUrl))
    {
        var response = await httpClient.GetAsync(requestUrl);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            // Log or handle errorContent as needed
            return new List<M365CopilotUsageReport>();
        }

        var jsonString = await response.Content.ReadAsStringAsync();
        var json = Newtonsoft.Json.Linq.JObject.Parse(jsonString);

        // The data is usually under the "value" property
        var records = json["value"];
        if (records != null)
        {
            foreach (var record in records)
            {
                // Map JSON to your class
                var usageReport = new M365CopilotUsageReport
                        {
                            UserId = string.Empty, // I will populate it later
                            UserDisplayName = (string)record["displayName"]!,
                            UserPrincipalName = (string)record["userPrincipalName"]!,
                            UserDepartment = (string)record["department"]!,
                            LastActivityDate = ParseNullableDateTime(record["lastActivityDate"]!),
                            CopilotChatLastActivityDate = ParseNullableDateTime(record["copilotChatLastActivityDate"]!),
                            MicrosoftTeamsCopilotLastActivityDate = ParseNullableDateTime(record["microsoftTeamsCopilotLastActivityDate"]!),
                            WordCopilotLastActivityDate = ParseNullableDateTime(record["wordCopilotLastActivityDate"]!),
                            ExcelCopilotLastActivityDate = ParseNullableDateTime(record["excelCopilotLastActivityDate"]!),
                            PowerPointCopilotLastActivityDate = ParseNullableDateTime(record["powerPointCopilotLastActivityDate"]!),
                            OutlookCopilotLastActivityDate = ParseNullableDateTime(record["outlookCopilotLastActivityDate"]!),
                            OneNoteCopilotLastActivityDate = ParseNullableDateTime(record["oneNoteCopilotLastActivityDate"]!),
                            LoopCopilotLastActivityDate = ParseNullableDateTime(record["loopCopilotLastActivityDate"]!)
                        };

                usageList.Add(usageReport);
            }
        }

        // Check for pagination
        requestUrl = json["@odata.nextLink"]?.ToString()!;
    }

    return usageList;
}


    private DateTime? ParseNullableDateTime(JToken  token)
    {
        // Handle null or undefined tokens
        if (token == null || token.Type == JTokenType.Null)
            return null;
        
        var str = token.ToString();
        
        // Handle empty strings or "undefined" values
        if (string.IsNullOrWhiteSpace(str) || str.Equals("undefined", StringComparison.OrdinalIgnoreCase))
            return null;
        
        if (DateTime.TryParse(str, out var dt))
            return dt;

        return null;
    }
}
