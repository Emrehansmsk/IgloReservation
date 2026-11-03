namespace IgloReservation.Api.DTOs.Users;

public class UserListItemResponse
{
    public string Id { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? UserName { get; set; }
    public IReadOnlyList<string> Roles { get; set; } = [];
}
