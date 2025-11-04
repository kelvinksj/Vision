using System.Security.Claims;
using TaskManagerWeb.Components.Helper;
using TaskManagerWeb.Components.Models.Authentication;
using TaskManagerWeb.Components.Service.Info;

namespace TaskManagerWeb.Components.Service.Authentication;
public class UserAccountService
{
    private readonly string className = "UserAccountService";
    private CommonLib commonLib = new CommonLib();
    private string fileName = "wwwroot/json/UserAccount.json";
    private bool isEncrypte = false;
    private List<UserAccount> users = new List<UserAccount>();

    public UserAccountService(DirectoryService DirectoryService)
    {
        try
        {
            if (!DirectoryService.GetBaseDirectory().Contains("Debug"))
                fileName = DirectoryService.GetBaseDirectory() + fileName;

            users.Add(new UserAccount
            {
                UserId = 1,
                UserName = "Admin",
                FirstName = "Admin",
                LastName = "Admin",
                UserRole = nameof(Role.Administrator),
                Password = "Aa12345*",
                Page = "Disable"
            });

            commonLib.JsonSerializeFile(
                users,
                fileName,
                IsEncrypted: isEncrypte
            );
            users = commonLib.JsonDeserializeFile<List<UserAccount>>(
                fileName, isEncrypte) ?? new List<UserAccount>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Class={className}-{commonLib.GetDateTime()}: UserAccountService err:[{ex.HResult}]{ex.Message}");
        }
    }

    public UserAccount? GetByUserName(string UserName)
    {
        return users.FirstOrDefault(c => c.UserName == UserName).DeepClone();
    }

    public UserAccount? GetByUserId(int UserId)
    {
        return users.FirstOrDefault(c => c.UserId == UserId).DeepClone();
    }

    public List<UserAccount> GetUserList(ClaimsPrincipal AuthUser)
    {
        List<UserAccount> user = [];

        if (AuthUser.IsInRole(nameof(Role.Administrator)))
        {
            if (AuthUser.Identity!.Name == "Admin" || AuthUser.Identity!.Name == "Administrator")
            {
                return users;
            }
            else
            {
                user = users.Where(
                    c => c.UserName != "Admin"
                    && c.UserName != "Administrator"
                ).ToList();
                return user;
            }
        }
        else
        {
            user = users.Where(c => c.UserName == AuthUser.Identity!.Name).ToList();
            return user;
        }
    }

    public string CheckBeforeSave(UserAccount User)
    {
        UserAccount? existingUser = users.FirstOrDefault(
            c => c.UserName == User.UserName && c.UserId != User.UserId
        );

        if (existingUser != null)
        {
            return "Username already exits";
        }

        return string.Empty;
    }

    public void AddUser(UserAccount User)
    {
        if (User != null)
        {
            int id = users.Count != 0 ? users.Max(c => c.UserId) + 1 : 1;
            users.Add(new UserAccount
            {
                UserId = id,
                UserName = User.UserName,
                FirstName = User.FirstName,
                LastName = User.LastName,
                UserRole = User.UserRole,
                Password = User.Password,
                Page = User.Page
            });
            commonLib.JsonSerializeFile(
                users,
                fileName,
                true,
                isEncrypte
            );
        }
    }

    public void SaveUserInfo(UserAccount User)
    {
        UserAccount? existingUser = users.FirstOrDefault(c => c.UserId == User.UserId);

        if (existingUser != null)
        {
            existingUser.UserName = User.UserName;
            existingUser.FirstName = User.FirstName;
            existingUser.LastName = User.LastName;
            existingUser.UserRole = User.UserRole;
            existingUser.Password = User.Password;
            existingUser.Page = User.Page;
            commonLib.JsonSerializeFile(
                users,
                fileName,
                true,
                isEncrypte
            );
        }
    }

    public void DeleteUserName(UserAccount User)
    {
        users.Remove(User);
        commonLib.JsonSerializeFile(
            users,
            fileName,
            true,
            isEncrypte
        );
    }
}