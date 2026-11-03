using System.Net.Mail;
using IgloReservation.Api.Configurations;
using IgloReservation.Api.Exceptions;
using IgloReservation.Api.Models.Entities;
using IgloReservation.Api.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace IgloReservation.Api.Services;

/// <summary>
/// Secure Admin bootstrap. Credentials must come from env / User Secrets (AdminBootstrap__Email / Password).
/// Never read a committed appsettings password. Never log passwords or other secrets.
/// </summary>
public class AdminBootstrapService : IAdminBootstrapService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly AdminBootstrapOptions _options;
    private readonly ILogger<AdminBootstrapService> _logger;

    public AdminBootstrapService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IOptions<AdminBootstrapOptions> options,
        ILogger<AdminBootstrapService> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _options = options.Value;
        _logger = logger;
    }

    public async Task EnsureAdminAsync(CancellationToken cancellationToken = default)
    {
        ValidateOptionsOrThrow();

        await EnsureRolesAsync();

        if (!_options.HasCompleteCredentials)
        {
            _logger.LogWarning(
                "Admin bootstrap: Admin user was not created because AdminBootstrap:Email and AdminBootstrap:Password are not set. Provide both via User Secrets or environment variables.");
            return;
        }

        var email = _options.Email!.Trim();
        var existing = await _userManager.FindByEmailAsync(email);
        if (existing is not null)
        {
            // Idempotent: never recreate, never change password, never alter roles on an existing user.
            _logger.LogInformation(
                "Admin bootstrap: user for the configured email already exists; password and roles were left unchanged.");
            return;
        }

        var admin = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true
        };

        var create = await _userManager.CreateAsync(admin, _options.Password!);
        if (!create.Succeeded)
        {
            // Identity error descriptions only — never include the password.
            throw new AdminBootstrapException(
                $"Admin bootstrap failed while creating the Admin user: {string.Join("; ", create.Errors.Select(e => e.Description))}");
        }

        var roleResult = await _userManager.AddToRoleAsync(admin, AppRoles.Admin);
        if (!roleResult.Succeeded)
        {
            throw new AdminBootstrapException(
                $"Admin bootstrap created the user but failed to assign the Admin role: {string.Join("; ", roleResult.Errors.Select(e => e.Description))}");
        }

        _logger.LogInformation("Admin bootstrap: created Admin user from User Secrets / environment configuration.");
    }

    private void ValidateOptionsOrThrow()
    {
        var emailSet = !string.IsNullOrWhiteSpace(_options.Email);
        var passwordSet = !string.IsNullOrWhiteSpace(_options.Password);

        if (emailSet != passwordSet)
        {
            throw new AdminBootstrapException(
                "AdminBootstrap is misconfigured: both AdminBootstrap:Email and AdminBootstrap:Password must be set together via User Secrets or environment variables.");
        }

        if (!emailSet)
        {
            return;
        }

        if (!IsValidEmail(_options.Email!))
        {
            throw new AdminBootstrapException(
                "AdminBootstrap:Email is invalid. Provide a valid email address via User Secrets or environment variables.");
        }
    }

    private async Task EnsureRolesAsync()
    {
        // Admin is required for bootstrap; User is required for public registration.
        foreach (var role in AppRoles.All)
        {
            if (await _roleManager.RoleExistsAsync(role))
            {
                continue;
            }

            var createRole = await _roleManager.CreateAsync(new IdentityRole(role));
            if (!createRole.Succeeded)
            {
                throw new AdminBootstrapException(
                    $"Failed to create role '{role}': {string.Join("; ", createRole.Errors.Select(e => e.Description))}");
            }
        }
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var parsed = new MailAddress(email.Trim());
            return string.Equals(parsed.Address, email.Trim(), StringComparison.OrdinalIgnoreCase);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
