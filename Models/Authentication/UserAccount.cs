using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace TaskManagerWeb.Components.Models.Authentication;
public class ValidRoleAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object? Value, ValidationContext validationContext)
    {
        string? roleString = Value as string;

        if (Enum.TryParse<Role>(roleString, out _))
            return ValidationResult.Success!;
        else
            return new ValidationResult($"Invalid role: {roleString}");
    }
}

public class UserAccount
{
    [Required(ErrorMessage = "User ID is required")]
    public int UserId { get; set; } = 0;

    [Required(ErrorMessage = "Please input Username")]
    [MinLength(4, ErrorMessage = "User name must be a minimum of 4 characters")]
    [MaxLength(15, ErrorMessage = "User name must be not more than 15 characters")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please input First Name")]
    [MaxLength(15, ErrorMessage = "First name must be not more than 15 characters")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please input Last Name")]
    [MaxLength(15, ErrorMessage = "Last name must be not more than 15 characters")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select Role")]
    [ValidRole]
    public string UserRole { get; set; } = nameof(Role.Disable);

    [Required(ErrorMessage = "Please input Password")]
    [MinLength(6, ErrorMessage = "Password must be a minimum of 6 characters")]
    [MaxLength(15, ErrorMessage = "Password must be not more than 15 characters")]
    /*[RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
                       ErrorMessage = "password contains at least one lowercase letter, one uppercase "
                       + "letter, one digit, one special character, and is at least 8 characters long")]*/
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select Page")]
    public string Page { get; set; } = "Disable";

    [JsonIgnore]
    [NotMapped]
    public string PageDescription { get; set; } = string.Empty;

    [JsonIgnore]
    [NotMapped]
    public bool IsBusy { get; set; } = false;

    [JsonIgnore]
    [NotMapped]
    public bool IsDelete { get; set; } = false;

    [JsonIgnore]
    [NotMapped]
    public bool IsEdit { get; set; } = false;

    [JsonIgnore]
    [NotMapped]
    public bool IsChecking { get; set; } = false;
}