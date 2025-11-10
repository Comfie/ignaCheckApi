using IgnaCheck.Application.Common.Interfaces;
using IgnaCheck.Application.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IgnaCheck.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserClaimsPrincipalFactory<ApplicationUser> _userClaimsPrincipalFactory;
    private readonly IAuthorizationService _authorizationService;

    public IdentityService(
        UserManager<ApplicationUser> userManager,
        IUserClaimsPrincipalFactory<ApplicationUser> userClaimsPrincipalFactory,
        IAuthorizationService authorizationService)
    {
        _userManager = userManager;
        _userClaimsPrincipalFactory = userClaimsPrincipalFactory;
        _authorizationService = authorizationService;
    }

    public async Task<string?> GetUserNameAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user?.UserName;
    }

    public async Task<(Result Result, string UserId)> CreateUserAsync(string userName, string password)
    {
        var user = new ApplicationUser
        {
            UserName = userName,
            Email = userName,
        };

        var result = await _userManager.CreateAsync(user, password);

        return (result.ToApplicationResult(), user.Id);
    }

    public async Task<bool> IsInRoleAsync(string userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user != null && await _userManager.IsInRoleAsync(user, role);
    }

    public async Task<bool> AuthorizeAsync(string userId, string policyName)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return false;
        }

        var principal = await _userClaimsPrincipalFactory.CreateAsync(user);

        var result = await _authorizationService.AuthorizeAsync(principal, policyName);

        return result.Succeeded;
    }

    public async Task<Result> DeleteUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user != null ? await DeleteUserAsync(user) : Result.Success();
    }

    public async Task<Result> DeleteUserAsync(ApplicationUser user)
    {
        var result = await _userManager.DeleteAsync(user);

        return result.ToApplicationResult();
    }

    // Enhanced user management methods

    public async Task<Result<string>> CreateUserAsync(
        string email,
        string password,
        string? firstName,
        string? lastName)
    {
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            CreatedDate = DateTime.UtcNow,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            return Result<string>.Failure(result.Errors.Select(e => e.Description));
        }

        return Result<string>.Success(user.Id);
    }

    public async Task<ApplicationUserDto?> GetUserByEmailAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        return user == null ? null : MapToDto(user);
    }

    public async Task<ApplicationUserDto?> GetUserByIdAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        return user == null ? null : MapToDto(user);
    }

    private static ApplicationUserDto MapToDto(ApplicationUser user)
    {
        return new ApplicationUserDto
        {
            Id = user.Id,
            Email = user.Email,
            UserName = user.UserName,
            FirstName = user.FirstName,
            LastName = user.LastName,
            AvatarUrl = user.AvatarUrl,
            JobTitle = user.JobTitle,
            Department = user.Department,
            PhoneNumber = user.PhoneNumber,
            PhoneNumberConfirmed = user.PhoneNumberConfirmed,
            TimeZone = user.TimeZone,
            PreferredLanguage = user.PreferredLanguage,
            NotificationPreferences = user.NotificationPreferences,
            EmailConfirmed = user.EmailConfirmed,
            IsActive = user.IsActive,
            InvitedDate = user.InvitedDate,
            RegistrationCompletedDate = user.RegistrationCompletedDate,
            LastLoginDate = user.LastLoginDate,
            Created = user.CreatedDate,
            LastModified = user.UpdatedDate,
            Bio = user.Bio
        };
    }

    // Email verification methods

    public async Task<string> GenerateEmailVerificationTokenAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        Guard.Against.Null(user, nameof(user), "User not found.");

        return await _userManager.GenerateEmailConfirmationTokenAsync(user);
    }

    public async Task<Result> VerifyEmailAsync(string userId, string token)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return Result.Failure(new[] { "User not found." });
        }

        var result = await _userManager.ConfirmEmailAsync(user, token);
        if (!result.Succeeded)
        {
            return Result.Failure(result.Errors.Select(e => e.Description));
        }

        // Update registration completed date
        user.RegistrationCompletedDate = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        return Result.Success();
    }

    // Password reset methods

    public async Task<string> GeneratePasswordResetTokenAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        Guard.Against.Null(user, nameof(user), "User not found.");

        return await _userManager.GeneratePasswordResetTokenAsync(user);
    }

    public async Task<Result> ResetPasswordAsync(string userId, string token, string newPassword)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return Result.Failure(new[] { "User not found." });
        }

        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        if (!result.Succeeded)
        {
            return Result.Failure(result.Errors.Select(e => e.Description));
        }

        return Result.Success();
    }

    // Authentication methods

    public async Task<string?> CheckPasswordAsync(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return null;
        }

        var isValid = await _userManager.CheckPasswordAsync(user, password);
        if (!isValid)
        {
            return null;
        }

        // Check if user is active
        if (!user.IsActive)
        {
            return null;
        }

        return user.Id;
    }

    public async Task UpdateLastLoginDateAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user != null)
        {
            user.LastLoginDate = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);
        }
    }

    // Profile management methods

    public async Task<bool> UpdateUserAvatarAsync(string userId, string avatarUrl)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return false;
        }

        user.AvatarUrl = avatarUrl;
        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
    }

    public async Task<bool> UpdateUserProfileAsync(
        string userId,
        string? firstName,
        string? lastName,
        string? jobTitle,
        string? department,
        string? phoneNumber,
        string? timeZone,
        string? preferredLanguage)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return false;
        }

        // Update fields if provided
        if (firstName != null)
        {
            user.FirstName = firstName;
        }

        if (lastName != null)
        {
            user.LastName = lastName;
        }

        if (jobTitle != null)
        {
            user.JobTitle = jobTitle;
        }

        if (department != null)
        {
            user.Department = department;
        }

        if (phoneNumber != null)
        {
            user.PhoneNumber = phoneNumber;
            // Reset phone number confirmation if changed
            if (user.PhoneNumber != phoneNumber)
            {
                user.PhoneNumberConfirmed = false;
            }
        }

        if (timeZone != null)
        {
            user.TimeZone = timeZone;
        }

        if (preferredLanguage != null)
        {
            user.PreferredLanguage = preferredLanguage;
        }

        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
    }

    public async Task<bool> UpdateNotificationPreferencesAsync(string userId, string notificationPreferences)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return false;
        }

        user.NotificationPreferences = notificationPreferences;
        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
    }

    public async Task<string?> GetNotificationPreferencesAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        return user?.NotificationPreferences;
    }
}
