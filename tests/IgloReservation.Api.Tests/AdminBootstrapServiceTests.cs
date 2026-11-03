using IgloReservation.Api.Configurations;
using IgloReservation.Api.Data;
using IgloReservation.Api.Exceptions;
using IgloReservation.Api.Models.Entities;
using IgloReservation.Api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace IgloReservation.Api.Tests;

public class AdminBootstrapServiceTests : IDisposable
{
    private readonly ServiceProvider _services;

    public AdminBootstrapServiceTests()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDataProtection();
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseInMemoryDatabase("AdminBootstrap_" + Guid.NewGuid().ToString("N")));

        services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 8;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        _services = services.BuildServiceProvider();
    }

    public void Dispose()
    {
        _services.Dispose();
    }

    [Fact]
    public async Task EnsureAdminAsync_MissingCredentials_CreatesRolesButNoUser()
    {
        var sut = CreateSut(email: null, password: null);

        await sut.EnsureAdminAsync();

        var roles = _services.GetRequiredService<RoleManager<IdentityRole>>();
        Assert.True(await roles.RoleExistsAsync(AppRoles.Admin));
        Assert.True(await roles.RoleExistsAsync(AppRoles.User));

        var users = _services.GetRequiredService<UserManager<ApplicationUser>>();
        Assert.Empty(users.Users.ToList());
    }

    [Fact]
    public async Task EnsureAdminAsync_PartialCredentials_ThrowsWithoutCreatingUser()
    {
        var sut = CreateSut(email: "admin@example.com", password: null);

        var ex = await Assert.ThrowsAsync<AdminBootstrapException>(() => sut.EnsureAdminAsync());
        Assert.Contains("must be set together", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task EnsureAdminAsync_InvalidEmail_Throws()
    {
        var sut = CreateSut(email: "not-an-email", password: "Str0ng!Pass");

        var ex = await Assert.ThrowsAsync<AdminBootstrapException>(() => sut.EnsureAdminAsync());
        Assert.Contains("invalid", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task EnsureAdminAsync_CreatesAdmin_WhenUserMissing()
    {
        var sut = CreateSut(email: "admin@example.com", password: "Str0ng!Passw0rd");

        await sut.EnsureAdminAsync();

        var users = _services.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await users.FindByEmailAsync("admin@example.com");
        Assert.NotNull(user);
        Assert.True(await users.IsInRoleAsync(user, AppRoles.Admin));
        Assert.False(await users.IsInRoleAsync(user, AppRoles.User));
    }

    [Fact]
    public async Task EnsureAdminAsync_ExistingUser_DoesNotChangePasswordOrRoles()
    {
        var users = _services.GetRequiredService<UserManager<ApplicationUser>>();
        var roles = _services.GetRequiredService<RoleManager<IdentityRole>>();
        await roles.CreateAsync(new IdentityRole(AppRoles.User));
        await roles.CreateAsync(new IdentityRole(AppRoles.Admin));

        var existing = new ApplicationUser
        {
            UserName = "admin@example.com",
            Email = "admin@example.com",
            EmailConfirmed = true
        };
        var create = await users.CreateAsync(existing, "Original!Pass1");
        Assert.True(create.Succeeded);
        await users.AddToRoleAsync(existing, AppRoles.User);

        var beforeHash = (await users.FindByEmailAsync("admin@example.com"))!.PasswordHash;

        var sut = CreateSut(email: "admin@example.com", password: "BrandNew!Pass9");
        await sut.EnsureAdminAsync();
        await sut.EnsureAdminAsync(); // idempotent second run

        var after = await users.FindByEmailAsync("admin@example.com");
        Assert.NotNull(after);
        Assert.Equal(beforeHash, after.PasswordHash);
        Assert.True(await users.IsInRoleAsync(after, AppRoles.User));
        Assert.False(await users.IsInRoleAsync(after, AppRoles.Admin));
        Assert.True(await users.CheckPasswordAsync(after, "Original!Pass1"));
        Assert.False(await users.CheckPasswordAsync(after, "BrandNew!Pass9"));
    }

    [Fact]
    public async Task EnsureAdminAsync_WeakPassword_ThrowsSafeMessage()
    {
        var sut = CreateSut(email: "admin@example.com", password: "short");

        var ex = await Assert.ThrowsAsync<AdminBootstrapException>(() => sut.EnsureAdminAsync());
        Assert.Contains("creating the Admin user", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("short", ex.Message, StringComparison.Ordinal);
    }

    private AdminBootstrapService CreateSut(string? email, string? password)
    {
        var options = Options.Create(new AdminBootstrapOptions
        {
            Email = email,
            Password = password
        });

        return new AdminBootstrapService(
            _services.GetRequiredService<UserManager<ApplicationUser>>(),
            _services.GetRequiredService<RoleManager<IdentityRole>>(),
            options,
            NullLogger<AdminBootstrapService>.Instance);
    }
}
