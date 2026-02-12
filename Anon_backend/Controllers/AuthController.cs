using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using FullstackTemplate.Application.Interfaces;
using FullstackTemplate.Application.DTOs;

namespace FullstackTemplate.Controllers;

[ApiController]
[Route("api/auth")]
[EnableRateLimiting("AuthLimit")]  // Stricter rate limiting for auth endpoints
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    // Auth0 namespace for custom claims - configure this to match your Auth0 tenant
    private const string Auth0RolesNamespace = "https://dataguard.app/roles";

    public AuthController(IUserService userService, IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _userService = userService;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    [HttpGet("user")]
    [Authorize]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        // DEBUG: Log all claims to diagnose RBAC issues
        Console.WriteLine("=== JWT CLAIMS DEBUG ===");
        foreach (var claim in User.Claims)
        {
            Console.WriteLine($"  {claim.Type}: {claim.Value}");
        }
        Console.WriteLine("========================");

        // Extract email from Auth0 JWT claims
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
                    ?? User.FindFirst("email")?.Value
                    ?? User.FindFirst($"{Auth0RolesNamespace}/email")?.Value;

        // User info fetched from Auth0 /userinfo endpoint
        Auth0UserInfo? userInfo = null;

        // If email not in token, fetch from Auth0 /userinfo endpoint
        if (string.IsNullOrEmpty(email))
        {
            var authHeader = Request.Headers["Authorization"].FirstOrDefault();
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                var accessToken = authHeader.Substring("Bearer ".Length);
                userInfo = await FetchAuth0UserInfoAsync(accessToken);
                email = userInfo?.Email;
                Console.WriteLine($"DEBUG: Fetched email from /userinfo: {email}");
            }
        }

        if (string.IsNullOrEmpty(email))
            return Unauthorized(new { error = "No email claim found in token or userinfo" });

        // Extract role from Auth0 custom claims namespace
        var roleClaim = User.FindFirst($"{Auth0RolesNamespace}")?.Value
                        ?? User.FindFirst("https://dataguard.app/role")?.Value;
        
        // Parse role from permissions if using Auth0 RBAC permissions
        var permissions = User.FindAll("permissions").Select(c => c.Value).ToList();
        Console.WriteLine($"DEBUG: Permissions found: [{string.Join(", ", permissions)}]");
        Console.WriteLine($"DEBUG: RoleClaim: {roleClaim}");
        var role = DetermineRoleFromPermissions(permissions, roleClaim);
        Console.WriteLine($"DEBUG: Determined role: {role}");

        var user = await _userService.GetByEmailAsync(email);
        if (user is null)
        {
            // Auto-provision user on first login - prefer userInfo from /userinfo endpoint
            var name = userInfo?.Name
                       ?? User.FindFirst("name")?.Value
                       ?? User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
            var firstName = userInfo?.GivenName
                       ?? User.FindFirst(System.Security.Claims.ClaimTypes.GivenName)?.Value;
            var lastName = userInfo?.FamilyName
                       ?? User.FindFirst(System.Security.Claims.ClaimTypes.Surname)?.Value;
            var picture = userInfo?.Picture
                       ?? User.FindFirst("picture")?.Value;

            user = await _userService.CreateAsync(new CreateUserDto(
                Email: email,
                FirstName: firstName,
                LastName: lastName,
                ProfileImageUrl: picture,
                Name: name ?? email,
                Role: role,
                Status: "active",
                InactiveDate: null,
                Department: null
            ));
        }
        else if (user.Role != role)
        {
            // Sync role from Auth0 if it changed
            user = await _userService.UpdateAsync(user.Id, new UpdateUserDto(
                Email: null,
                FirstName: null,
                LastName: null,
                ProfileImageUrl: null,
                Name: null,
                Role: role,
                Status: null,
                InactiveDate: null,
                Department: null
            ));
        }

        return Ok(user);
    }

    /// <summary>
    /// Determines user role based on Auth0 permissions.
    /// Order matters: check most permissive first.
    /// </summary>
    private static string DetermineRoleFromPermissions(List<string> permissions, string? explicitRole)
    {
        // If explicit role claim exists, use it
        if (!string.IsNullOrEmpty(explicitRole))
        {
            var normalized = explicitRole.ToLowerInvariant();
            if (new[] { "admin", "developer", "scheduler", "viewer" }.Contains(normalized))
                return normalized;
        }

        // Derive role from permissions
        if (permissions.Contains("manage:users"))
            return "admin";
        if (permissions.Contains("manage:configurations"))
            return "developer";
        if (permissions.Contains("manage:jobs"))
            return "scheduler";
        
        return "viewer";
    }

    /// <summary>
    /// Fetches user info from Auth0's /userinfo endpoint using the access token.
    /// </summary>
    private async Task<Auth0UserInfo?> FetchAuth0UserInfoAsync(string accessToken)
    {
        try
        {
            var auth0Domain = _configuration["Auth0:Domain"];
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            
            var response = await client.GetAsync($"https://{auth0Domain}/userinfo");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"DEBUG: Auth0 /userinfo response: {json}");
                return JsonSerializer.Deserialize<Auth0UserInfo>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            
            Console.WriteLine($"DEBUG: Auth0 /userinfo failed: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"DEBUG: Error fetching Auth0 userinfo: {ex.Message}");
        }
        return null;
    }
}

/// <summary>
/// Auth0 userinfo response model.
/// </summary>
public class Auth0UserInfo
{
    public string? Sub { get; set; }
    public string? Email { get; set; }
    public bool? EmailVerified { get; set; }
    public string? Name { get; set; }
    public string? GivenName { get; set; }
    public string? FamilyName { get; set; }
    public string? Picture { get; set; }
    public string? Locale { get; set; }
}
