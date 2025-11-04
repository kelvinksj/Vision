using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Security.Claims;
using TaskManagerWeb.Components.Models.Authentication;
using TaskManagerWeb.Components.Helper;

namespace TaskManagerWeb.Components.Service.Authentication;
public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly string taskName = "CustomAuthenticationStateProvider";
    private MessageLevel settingLevel => DebugParameters.MsgLvl_ServCustom;
    private CommonLib commonLib = new CommonLib();
    private readonly ProtectedSessionStorage sessionStorage;
    private ClaimsPrincipal anonymous = new ClaimsPrincipal(new ClaimsIdentity());

    public CustomAuthenticationStateProvider(ProtectedSessionStorage SessionStorage)
    {
        sessionStorage = SessionStorage;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        commonLib?.DisplayConsole(
            taskName,
            $"GetAuthenticationStateAsync triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            ProtectedBrowserStorageResult<UserSession> userSessionStorageResult = new ProtectedBrowserStorageResult<UserSession>();

            try
            {
                userSessionStorageResult = await sessionStorage.GetAsync<UserSession>("UserSession");
                commonLib?.DisplayConsole(
                    taskName,
                    $"GetAuthenticationStateAsync sessionStorage done",
                    settingLevel,
                    MessageLevel.Information
                );
            }
            catch (Exception ex)
            {
                commonLib?.DisplayConsole(
                    taskName,
                    ex,
                    settingLevel,
                    MessageLevel.Error
                );
            }

            UserSession? userSession = userSessionStorageResult.Success ? userSessionStorageResult.Value : null;

            if (userSession == null)
            {
                commonLib?.DisplayConsole(
                    taskName,
                    $"GetAuthenticationStateAsync invalid user!",
                    settingLevel,
                    MessageLevel.Information
                );
                return await Task.FromResult(new AuthenticationState(anonymous));
            }

            ClaimsPrincipal? claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
                {
                    new Claim(ClaimTypes.SerialNumber, userSession.UserId),
                    new Claim(ClaimTypes.Name, userSession.UserName),
                    new Claim(ClaimTypes.GivenName, userSession.FirstName),
                    new Claim(ClaimTypes.Surname, userSession.LastName),
                    new Claim(ClaimTypes.Role, userSession.UserRole),
                    new Claim(ClaimTypes.GroupSid, userSession.Page)
                }, "CustomAuth"));

            commonLib?.DisplayConsole(
                taskName,
                $"GetAuthenticationStateAsync done",
                settingLevel,
                MessageLevel.Information
            );
            return await Task.FromResult(new AuthenticationState(claimsPrincipal));
        }
        catch (Exception ex)
        {
            commonLib?.DisplayConsole(
                taskName,
                ex,
                settingLevel,
                MessageLevel.Error
            );
            return await Task.FromResult(new AuthenticationState(anonymous));
        }
    }

    public async Task UpdateAuthenticationState(UserSession UserSession)
    {
        commonLib?.DisplayConsole(
            taskName,
            $"UpdateAuthenticationState triggered",
            settingLevel,
            MessageLevel.Information
        );

        ClaimsPrincipal claimsPrincipal;

        try
        {
            if (UserSession != null)
            {
                await sessionStorage.SetAsync("UserSession", UserSession);
                claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
                    {
                        new Claim(ClaimTypes.SerialNumber, UserSession.UserId),
                        new Claim(ClaimTypes.GivenName, UserSession.FirstName),
                        new Claim(ClaimTypes.Surname, UserSession.LastName),
                        new Claim(ClaimTypes.Name, UserSession.UserName),
                        new Claim(ClaimTypes.Role, UserSession.UserRole),
                        new Claim(ClaimTypes.GroupSid, UserSession.Page)
                    }, "CustomAuth"));
                commonLib?.DisplayConsole(
                    taskName,
                    $"UpdateAuthenticationState DeleteAsync done",
                    settingLevel,
                    MessageLevel.Information
                );
            }
            else
            {
                await sessionStorage.DeleteAsync("UserSession");
                claimsPrincipal = anonymous;
                commonLib?.DisplayConsole(
                    taskName,
                    $"UpdateAuthenticationState DeleteAsync done",
                    settingLevel,
                    MessageLevel.Information
                );
            }

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(claimsPrincipal)));
            commonLib?.DisplayConsole(
                taskName,
                $"UpdateAuthenticationState done",
                settingLevel,
                MessageLevel.Information
            );
        }
        catch (Exception ex)
        {
            commonLib?.DisplayConsole(
                taskName,
                ex,
                settingLevel,
                MessageLevel.Error
            );
        }
    }
}