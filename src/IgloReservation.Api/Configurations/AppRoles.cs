namespace IgloReservation.Api.Configurations;

/// <summary>
/// Identity role names. Public register always assigns <see cref="User"/>; never Admin.
/// </summary>
public static class AppRoles
{
    public const string Admin = "Admin";
    public const string User = "User";

    public static readonly string[] All = [Admin, User];
}
