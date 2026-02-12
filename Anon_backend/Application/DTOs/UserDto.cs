using System.ComponentModel.DataAnnotations;

namespace FullstackTemplate.Application.DTOs;

public record UserDto(
    Guid Id,
    string? Email,
    string? FirstName,
    string? LastName,
    string? ProfileImageUrl,
    string? Name,
    string Role,
    string Status,
    DateOnly? InactiveDate,
    string? Department,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record CreateUserDto(
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [MaxLength(256, ErrorMessage = "Email must be 256 characters or less")]
    string? Email,
    
    [MaxLength(100, ErrorMessage = "First name must be 100 characters or less")]
    [RegularExpression(@"^[a-zA-Z\s\-']+$", ErrorMessage = "First name contains invalid characters")]
    string? FirstName,
    
    [MaxLength(100, ErrorMessage = "Last name must be 100 characters or less")]
    [RegularExpression(@"^[a-zA-Z\s\-']+$", ErrorMessage = "Last name contains invalid characters")]
    string? LastName,
    
    [Url(ErrorMessage = "Invalid URL format")]
    [MaxLength(2048, ErrorMessage = "Profile image URL must be 2048 characters or less")]
    string? ProfileImageUrl,
    
    [MaxLength(200, ErrorMessage = "Name must be 200 characters or less")]
    string? Name,
    
    [RegularExpression(@"^(admin|developer|scheduler|viewer)$", ErrorMessage = "Invalid role")]
    string? Role,
    
    [RegularExpression(@"^(active|inactive|pending)$", ErrorMessage = "Invalid status")]
    string? Status,
    
    DateOnly? InactiveDate,
    
    [MaxLength(100, ErrorMessage = "Department must be 100 characters or less")]
    string? Department
);

public record UpdateUserDto(
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [MaxLength(256, ErrorMessage = "Email must be 256 characters or less")]
    string? Email,
    
    [MaxLength(100, ErrorMessage = "First name must be 100 characters or less")]
    [RegularExpression(@"^[a-zA-Z\s\-']*$", ErrorMessage = "First name contains invalid characters")]
    string? FirstName,
    
    [MaxLength(100, ErrorMessage = "Last name must be 100 characters or less")]
    [RegularExpression(@"^[a-zA-Z\s\-']*$", ErrorMessage = "Last name contains invalid characters")]
    string? LastName,
    
    [Url(ErrorMessage = "Invalid URL format")]
    [MaxLength(2048, ErrorMessage = "Profile image URL must be 2048 characters or less")]
    string? ProfileImageUrl,
    
    [MaxLength(200, ErrorMessage = "Name must be 200 characters or less")]
    string? Name,
    
    [RegularExpression(@"^(admin|developer|scheduler|viewer)$", ErrorMessage = "Invalid role")]
    string? Role,
    
    [RegularExpression(@"^(active|inactive|pending)$", ErrorMessage = "Invalid status")]
    string? Status,
    
    DateOnly? InactiveDate,
    
    [MaxLength(100, ErrorMessage = "Department must be 100 characters or less")]
    string? Department
);
