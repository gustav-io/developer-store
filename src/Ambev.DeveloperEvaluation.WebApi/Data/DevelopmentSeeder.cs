using Ambev.DeveloperEvaluation.Common.Security;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.ORM;
using Microsoft.EntityFrameworkCore;

namespace Ambev.DeveloperEvaluation.WebApi.Data;

/// <summary>
/// Applies pending migrations and seeds a development admin so the API can be exercised right away.
/// Only wired up for the Development environment.
/// </summary>
public static class DevelopmentSeeder
{
    public const string AdminEmail = "admin@developerstore.local";
    public const string AdminPassword = "Admin@123";

    public static async Task RunAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<DefaultContext>();
        await context.Database.MigrateAsync();

        var users = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        if (await users.GetByEmailAsync(AdminEmail) != null)
            return;

        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        await users.CreateAsync(new User
        {
            Username = "admin",
            Email = AdminEmail,
            Phone = "+5511999999999",
            Password = hasher.HashPassword(AdminPassword),
            Role = UserRole.Admin,
            Status = UserStatus.Active
        });
    }
}
