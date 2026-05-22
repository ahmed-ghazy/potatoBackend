using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PotatoAccounting.Domain.Entities;
using PotatoAccounting.Infrastructure.Persistence;

namespace PotatoAccounting.Infrastructure.Identity;

public static class IdentityDataSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var db = services.GetRequiredService<ApplicationDbContext>();
        var configuration = services.GetRequiredService<IConfiguration>();
        var logger = services.GetRequiredService<ILogger<ApplicationUser>>();

        // Roles
        foreach (var role in new[] { "SuperAdmin", "Admin", "User" })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        // SuperAdmin user
        await SeedSuperAdminAsync(userManager, configuration, logger);

        // Default tenant + its first admin
        await SeedDefaultTenantAsync(userManager, db, configuration, logger);
    }

    private static async Task SeedSuperAdminAsync(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        ILogger logger)
    {
        var s = configuration.GetSection("DefaultSuperAdmin").Get<DefaultSuperAdminSettings>();
        if (s is null || string.IsNullOrWhiteSpace(s.Password))
        {
            logger.LogWarning("DefaultSuperAdmin settings missing — skipping SuperAdmin seed.");
            return;
        }

        // Prefer phone as the user identifier; fall back to email so legacy configs still work.
        var phone = s.PhoneNumber?.Trim();
        var email = s.Email?.Trim();
        var identifier = !string.IsNullOrEmpty(phone) ? phone : email;
        if (string.IsNullOrWhiteSpace(identifier))
        {
            logger.LogWarning("DefaultSuperAdmin needs PhoneNumber or Email — skipping seed.");
            return;
        }

        var existing = await FindExistingAsync(userManager, phone, email);
        if (existing is not null) return;

        var user = new ApplicationUser
        {
            UserName = identifier,
            Email = string.IsNullOrEmpty(email) ? null : email,
            EmailConfirmed = !string.IsNullOrEmpty(email),
            PhoneNumber = string.IsNullOrEmpty(phone) ? null : phone,
            PhoneNumberConfirmed = !string.IsNullOrEmpty(phone),
            FullName = string.IsNullOrWhiteSpace(s.FullName) ? "Super Admin" : s.FullName,
            TenantId = null,
        };
        var result = await userManager.CreateAsync(user, s.Password);
        if (!result.Succeeded)
        {
            logger.LogError("Failed to seed SuperAdmin: {Errors}", string.Join("; ", result.Errors.Select(e => e.Description)));
            return;
        }
        await userManager.AddToRoleAsync(user, "SuperAdmin");
        logger.LogInformation("SuperAdmin '{Identifier}' seeded.", identifier);
    }

    private static async Task SeedDefaultTenantAsync(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext db,
        IConfiguration configuration,
        ILogger logger)
    {
        var s = configuration.GetSection("DefaultTenant").Get<DefaultTenantSettings>();
        if (s is null || string.IsNullOrWhiteSpace(s.Name))
        {
            logger.LogWarning("DefaultTenant settings missing — skipping default tenant seed.");
            return;
        }

        var phone = s.AdminPhoneNumber?.Trim();
        var email = s.AdminEmail?.Trim();
        var identifier = !string.IsNullOrEmpty(phone) ? phone : email;
        if (string.IsNullOrWhiteSpace(identifier))
        {
            logger.LogWarning("DefaultTenant admin needs AdminPhoneNumber or AdminEmail — skipping seed.");
            return;
        }

        // Migration already inserted a default tenant. Look for it by name; fall back to first tenant.
        var tenant = await db.Tenants.IgnoreQueryFilters()
                            .FirstOrDefaultAsync(t => t.Name == s.Name)
                     ?? await db.Tenants.IgnoreQueryFilters().OrderBy(t => t.Id).FirstOrDefaultAsync();

        if (tenant is null)
        {
            tenant = Tenant.Create(s.Name, createdBy: null);
            db.Tenants.Add(tenant);
            await db.SaveChangesAsync();
        }

        // If admin already exists (matched by phone or email), just attach them to the tenant.
        var existing = await FindExistingAsync(userManager, phone, email);
        if (existing is not null)
        {
            existing.TenantId ??= tenant.Id;
            if (!string.IsNullOrEmpty(phone) && string.IsNullOrEmpty(existing.PhoneNumber))
            {
                existing.PhoneNumber = phone;
                existing.PhoneNumberConfirmed = true;
            }
            await userManager.UpdateAsync(existing);
            var roles = await userManager.GetRolesAsync(existing);
            if (!roles.Contains("Admin"))
                await userManager.AddToRoleAsync(existing, "Admin");
            logger.LogInformation("Default tenant '{Name}' created; existing admin '{Identifier}' attached.", s.Name, identifier);
            return;
        }

        var user = new ApplicationUser
        {
            UserName = identifier,
            Email = string.IsNullOrEmpty(email) ? null : email,
            EmailConfirmed = !string.IsNullOrEmpty(email),
            PhoneNumber = string.IsNullOrEmpty(phone) ? null : phone,
            PhoneNumberConfirmed = !string.IsNullOrEmpty(phone),
            FullName = string.IsNullOrWhiteSpace(s.AdminFullName) ? "Admin" : s.AdminFullName,
            TenantId = tenant.Id,
        };
        var result = await userManager.CreateAsync(user, s.AdminPassword);
        if (!result.Succeeded)
        {
            logger.LogError("Failed to seed default tenant admin: {Errors}", string.Join("; ", result.Errors.Select(e => e.Description)));
            return;
        }
        await userManager.AddToRoleAsync(user, "Admin");
        logger.LogInformation("Default tenant '{Name}' + admin '{Identifier}' seeded.", s.Name, identifier);
    }

    private static async Task<ApplicationUser?> FindExistingAsync(
        UserManager<ApplicationUser> userManager,
        string? phone,
        string? email)
    {
        if (!string.IsNullOrEmpty(phone))
        {
            var byPhoneName = await userManager.FindByNameAsync(phone);
            if (byPhoneName is not null) return byPhoneName;
            var byPhone = await userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phone);
            if (byPhone is not null) return byPhone;
        }
        if (!string.IsNullOrEmpty(email))
        {
            var byEmail = await userManager.FindByEmailAsync(email);
            if (byEmail is not null) return byEmail;
        }
        return null;
    }
}
