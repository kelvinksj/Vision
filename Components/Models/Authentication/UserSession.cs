namespace TaskManagerWeb.Components.Models.Authentication;
public enum Role
{
    Disable = 0,
    Operator = 1,
    Supervisor = 100,
    Engineer = 300,
    Administrator = 500
}

public class UserSession
{
    public string UserId { get; set; } = "1";
    public string FirstName { get; set; } = "admin";
    public string LastName { get; set; } = "admin";
    public string UserName { get; set; } = string.Empty;
    public string UserRole { get; set; } = nameof(Role.Disable);
    public string Page { get; set; } = string.Empty;
}