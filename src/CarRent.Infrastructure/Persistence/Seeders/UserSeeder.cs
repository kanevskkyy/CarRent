using CarRent.Application.Interfaces.Services;
using CarRent.Domain.Entities;
using CarRent.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CarRent.Infrastructure.Persistence.Seeders;

public class UserSeeder : IDbSeeder
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly ILogger<UserSeeder> _logger;

    public UserSeeder(
        UserManager<User> userManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        ILogger<UserSeeder> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await SeedRolesAsync();
        await SeedUsersAsync();
    }

    private async Task SeedRolesAsync()
    {
        string[] roles = ["Admin", "User"];

        foreach (string roleName in roles)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                IdentityResult roleResult = await _roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
                if (roleResult.Succeeded)
                {
                    _logger.LogInformation("Role '{RoleName}' successfully created.", roleName);
                }
                else
                {
                    _logger.LogError("Failed to create role '{RoleName}': {Errors}", 
                        roleName, string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                }
            }
        }
    }

    private async Task SeedUsersAsync()
    {
        if (await _userManager.Users.AnyAsync())
        {
            _logger.LogInformation("Users already exist in the database. Skipping user seeding.");
            return;
        }

        // 1. Seed Admin
        string adminEmail = "admin@carrent.com";
        User adminUser = new()
        {
            Id = Guid.NewGuid(),
            UserName = "admin@carrent.com",
            NormalizedUserName = adminEmail.ToUpperInvariant(),
            Email = adminEmail,
            NormalizedEmail = adminEmail.ToUpperInvariant(),
            EmailConfirmed = true,
            FullName = "Admin User",
            DriverLicenseNumber = "ADM-777888",
            LicenseStatus = DriverLicenseVerificationStatus.Verified
        };

        IdentityResult createAdminResult = await _userManager.CreateAsync(adminUser, "Admin123!");
        if (createAdminResult.Succeeded)
        {
            await _userManager.AddToRoleAsync(adminUser, "Admin");
            _logger.LogInformation("Admin user ({Email}) seeded successfully with role 'Admin'.", adminEmail);
        }
        else
        {
            _logger.LogError("Failed to seed admin user: {Errors}", 
                string.Join(", ", createAdminResult.Errors.Select(e => e.Description)));
        }

        // 2. Seed Regular User
        string userEmail = "user@carrent.com";
        User regularUser = new()
        {
            Id = Guid.NewGuid(),
            UserName = "user@carrent.com",
            NormalizedUserName = userEmail.ToUpperInvariant(),
            Email = userEmail,
            NormalizedEmail = userEmail.ToUpperInvariant(),
            EmailConfirmed = true,
            FullName = "Standard User",
            DriverLicenseNumber = "USR-123456",
            LicenseStatus = DriverLicenseVerificationStatus.Verified
        };

        IdentityResult createUserResult = await _userManager.CreateAsync(regularUser, "User123!");
        if (createUserResult.Succeeded)
        {
            await _userManager.AddToRoleAsync(regularUser, "User");
            _logger.LogInformation("Regular user ({Email}) seeded successfully with role 'User'.", userEmail);
        }
        else
        {
            _logger.LogError("Failed to seed regular user: {Errors}", 
                string.Join(", ", createUserResult.Errors.Select(e => e.Description)));
        }
    }
}
