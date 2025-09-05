using Clara.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clara.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CopilotController : ControllerBase
{
    
    private readonly ICopilotLicenseService _licenseService;
    private readonly ICopilotUsageService _usageService;
    private readonly ICopilotGroupService _groupService;

    
    public CopilotController(ICopilotLicenseService licenseService,
                             ICopilotUsageService usageService,
                             ICopilotGroupService groupService)
    {
        
        _licenseService = licenseService;
        _usageService = usageService;
        _groupService = groupService;
    }

    [Authorize]
    [HttpGet("license-counts")]
    public async Task<IActionResult> GetCopilotLicenseAvailability()
    {        
        var counts = await _licenseService.GetLicenseCountsAsync();
        return Ok(counts);       
    }

    // List users with Copilot license
    [HttpGet("usage-report")]
    [Authorize]
    public async Task<IActionResult> GetM365CopilotUsageReport(int? days = null)
    {
        var inactive = await _usageService.GetInactiveUsersAsync(days);
        return Ok(inactive);
    }

    [Authorize]

    
    // Remove Copilot license from user
    [Authorize]
    [HttpPost("remove-license/{userId}")]
    public async Task<IActionResult> RemoveCopilotLicense(string userId)
    {
        bool result;
        
        // Check if userId looks like an email (contains @ symbol)
        if (userId.Contains("@"))
        {
            result = await _licenseService.RemoveLicenseByEmailAsync(userId);
            if (!result)
                return NotFound(new { message = $"User with email {userId} not found." });
        }
        else
        {
            result = await _licenseService.RemoveLicenseByIdAsync(userId);
            if (!result)
                return NotFound(new { message = $"User with id {userId} not found." });
        }

        return Ok(new { userId, removed = true });
    }


    [Authorize]
    [HttpPost("remove-license-by-email/{userEmail}")]
    public async Task<IActionResult> RemoveCopilotLicenseByEmail(string userEmail)
    {
        var result = await _licenseService.RemoveLicenseByEmailAsync(userEmail);
        if (!result)
            return NotFound(new { message = $"User with email {userEmail} not found." });

        return Ok(new { userEmail, removed = true });        
    }

    [Authorize]
    [HttpPost("assign-license/{userId}")]
    public async Task<IActionResult> AssignCopilotLicense(string userId)
    {
        bool result;
        
        // Check if userId looks like an email (contains @ symbol)
        if (userId.Contains("@"))
        {
            result = await _licenseService.AssignLicenseByEmailAsync(userId);
            if (!result)
                return NotFound(new { message = $"User with email {userId} not found." });
        }
        else
        {
            result = await _licenseService.AssignLicenseByIdAsync(userId);
            if (!result)
                return NotFound(new { message = $"User with id {userId} not found." });
        }

        return Ok(new { userId, assigned = true });

    }

    [Authorize]
    [HttpPost("assign-license-by-email/{userEmail}")]
    public async Task<IActionResult> AssignCopilotLicenseByEmail(string userEmail)
    {
        
        var result = await _licenseService.AssignLicenseByEmailAsync(userEmail);

        if (!result)
            return NotFound(new { message = $"User with email {userEmail} not found." });

        return Ok(new { userEmail, assigned = true });

    }


    [Authorize]
    [HttpPost("add-user-to-group/{userEmail}")]
    public async Task<IActionResult> AddUserToGroup(string userEmail)
    {   
        var result = await _groupService.AddUserToGroupAsync(userEmail);

        if (!result)
            return NotFound(new { message = $"User or group not found." });

        return Ok(new { userEmail, added = true });
       
    }

    [Authorize]
    [HttpPost("remove-user-from-group/{userEmail}")]
    public async Task<IActionResult> RemoveUserFromGroup(string userEmail)
    {
    
        var result = await _groupService.RemoveUserFromGroupAsync(userEmail);
        if (!result)
            return NotFound(new { message = $"User or group not found." });

        return Ok(new { userEmail, removed = true });

    }

    /// <summary>
    /// Get detailed usage analytics for a specific user
    /// </summary>
    /// <param name="userId">The user ID to get analytics for</param>
    /// <param name="days">Number of days to analyze (default: 30)</param>
    [HttpGet("user-analytics/{userId}")]
    [Authorize]
    public async Task<IActionResult> GetUserAnalytics(string userId, [FromQuery] int days = 30)
    {
        try
        {
            var analytics = await _usageService.GetUserUsageAnalyticsAsync(userId, days);
            return Ok(analytics);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving user analytics", error = ex.Message });
        }
    }
    
    /// <summary>
    /// Get usage analytics for all users with Copilot licenses
    /// </summary>
    /// <param name="days">Number of days to analyze (default: 30)</param>
    [HttpGet("all-users-analytics")]
    [Authorize]
    public async Task<IActionResult> GetAllUsersAnalytics([FromQuery] int days = 30)
    {
        try
        {
            var analytics = await _usageService.GetAllUsersAnalyticsAsync(days);
            return Ok(analytics);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving all users analytics", error = ex.Message });
        }
    }
    
    /// <summary>
    /// Get comprehensive usage summary report
    /// </summary>
    /// <param name="days">Number of days to analyze (default: 30)</param>
    [HttpGet("usage-summary")]
    [Authorize]
    public async Task<IActionResult> GetUsageSummary([FromQuery] int days = 30)
    {
        try
        {
            var summary = await _usageService.GetUsageSummaryReportAsync(days);
            return Ok(summary);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving usage summary", error = ex.Message });
        }
    }
    
    /// <summary>
    /// Get top users by Copilot usage
    /// </summary>
    /// <param name="topCount">Number of top users to return (default: 10)</param>
    /// <param name="days">Number of days to analyze (default: 30)</param>
    [HttpGet("top-users")]
    [Authorize]
    public async Task<IActionResult> GetTopUsers([FromQuery] int topCount = 10, [FromQuery] int days = 30)
    {
        try
        {
            var topUsers = await _usageService.GetTopUsersAsync(topCount, days);
            return Ok(topUsers);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving top users", error = ex.Message });
        }
    }
    
    /// <summary>
    /// Get usage analytics for users in a specific department
    /// </summary>
    /// <param name="department">Department name</param>
    /// <param name="days">Number of days to analyze (default: 30)</param>
    [HttpGet("department-analytics/{department}")]
    [Authorize]
    public async Task<IActionResult> GetDepartmentAnalytics(string department, [FromQuery] int days = 30)
    {
        try
        {
            var departmentUsers = await _usageService.GetUsersByDepartmentAsync(department, days);
            return Ok(departmentUsers);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving department analytics", error = ex.Message });
        }
    }


}
