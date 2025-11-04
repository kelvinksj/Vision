using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Routing;
using System.Security.Claims;
using TaskManagerWeb.Components.Helper;
using TaskManagerWeb.Components.Models.Authentication;
using TaskManagerWeb.Components.Models.Common;
using TaskManagerWeb.Components.Models.Mattel;
using TaskManagerWeb.Components.Service.Authentication;

/* Info -----------------------------------------------------------------
(Highest) Administrator->Engineer->Supervisor->Operator (lowest)

Home page
	- Administrator
	- Engineer
	- Supervisor
	- Operator

Location Status page
	- Administrator
	- Engineer
	- Supervisor
	
Caller page
	- Administrator
	- Engineer
	- Supervisor
	- Operator

Location Group page
	- Administrator
	- Engineer

User Info page
	- Administrator (User Name: Administrator) [Default account]
		. Always exist cannot be deleted
		. No other users can see this account
		. The highest user role
		. Add new users
		. Edit users
		. Can change own account password only
	- Administrator (User Name: {Others})
		. Can see other Administrator accounts and below (except the default Administrator)
		. Add new users [Administrator role and below]
		. Edit users (except user name) [Administrator role and below]
		. Delete users [Administrator role and below]
		. Can change own account info (except user name and user role)
		. Cannot delete own account
	- Engineer
		. Can see other Engineer accounts and below
		. Add new users [Engineer role and below]
		. Edit users (except user name) [Engineer role and below]
		. Delete users [Engineer role and below]
		. Can change own account (except user name and user role)
		. Cannot delete own account
	- Supervisor
		. Cannot see other user accounts
		. Can change own account info (except user name and user role)
		. Cannot delete own account
	- Operator
		. Cannot see other user accounts
		. Can change own account info (except user name and user role)
		. Cannot delete own account

Settings page
	- Administrator
	- Engineer
----------------------------------------------------------------- Info */

namespace TaskManagerWeb.Components.Pages.Account;
public partial class Users : IDisposable
{
    private bool showModal = false;
    private static bool showSuccessModal = false;
    private string taskName = "Users";
    private MessageLevel settingLevel => DebugParameters.MsgLvl_AccUser;
    private string currentUrl = string.Empty;
    private string? urlPage = string.Empty;
    private bool disposed = false;
    private ClaimsPrincipal authUser = new ClaimsPrincipal();
    private string? currentUserName = string.Empty;
    private int currentUserID = 0;
    private static List<UserAccount> users = [];
    private UserAccount user = new();
    private bool loading = false;
    private bool disableAddButton = false;
    private IDisposable? registration;
    private string workerNo = string.Empty;
    private string? modalContent = string.Empty;
    private static string? successContent = string.Empty;
    private int selectedUserId = 0;
    private PopMsgData popMsgData = new();
    private UserAccount selectedUser = null!;
    private MattelClass.PageList page = new();

    private bool disableEditButton =>
        (
            selectedUser == null
        );
    private string backgroundColor =>
        (
            selectedUser == null
        )
        ? "background-color: #a5a8a9;"
        : "background-color: #f36d33;";
    private string borderColor =>
        (
            selectedUser == null
        )
        ? "border-color: #a5a8a9;"
        : "border-color: #f36d33;";

    private bool disableDeleteButton =>
        (
            selectedUser == null
            || selectedUser?.UserName == currentUserName
        );
    private string bgColorDelete =>
        (
            selectedUser == null
            || selectedUser?.UserName == currentUserName
        )
        ? "background-color: #a5a8a9;"
        : "background-color: #f36d33;";
    private string borderColorDelete =>
        (
            selectedUser == null
            || selectedUser?.UserName == currentUserName
        )
        ? "border-color: #a5a8a9;"
        : "border-color: #f36d33;";

    private List<UserAccount>? filteredUsers = new();

    public Dictionary<string, string> SortIcons { get; set; } = new Dictionary<string, string>
    {
        {"UserName", "icon_filter"},
        {"FirstName", "icon_filter"},
        {"LastName", "icon_filter"},
        {"UserRole", "icon_filter"},
        {"PageDescription", "icon_filter"}
    };

    public Dictionary<string, bool> SortAscending { get; set; } = new Dictionary<string, bool>
    {
        {"UserName", true},
        {"FirstName", true},
        {"LastName", true},
        {"UserRole", true},
        {"PageDescription", true}
    };

    protected override void OnInitialized()
    {
        CommonLib.DisplayConsole(
            taskName,
            $"OnInitialized triggered",
            settingLevel,
            MessageLevel.Information
        );

        UpdateInfoChange();

        // Subscribe to URL customize LocationChanged event
        NavigationManager.LocationChanged += OnLocationChanged;
        loading = true;
        base.OnInitialized();
    }

    protected override async Task OnInitializedAsync()
    {
        CommonLib.DisplayConsole(
            taskName,
            $"OnInitializedAsync triggered",
            settingLevel,
            MessageLevel.Information
        );
        AuthenticationState authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        authUser = authState.User;
        currentUserName = authUser.Identity!.Name;

        if (!authUser.IsInRole("Administrator")
        && !authUser.IsInRole("Engineer")
        && !authUser.IsInRole("Supervisor")
        && !authUser.IsInRole("Operator"))
            NavigationManager.NavigateTo("/Account/UnauthorizedError", true, true);

        IList<Claim> authClaims = authUser.Claims.ToList();

        user = UserAccountService.GetByUserName(currentUserName!)!;
        _ = int.TryParse(
            authClaims.FirstOrDefault(c => c.Type == ClaimTypes.SerialNumber)!.Value,
            out currentUserID
        );

        users = UserAccountService.GetUserList(authUser);
        page = MattelService.GetPageList();

        foreach (var user in users)
        {
            var findDescription = page.PageTypeList.FirstOrDefault(
                n => n.Type == user.Page
            );

            if (findDescription != null)
                user.PageDescription = findDescription.Description;
            else
                user.PageDescription = string.Empty;
        }

        Sort();
        loading = false;
        CommonLib.DisplayConsole(
            taskName,
            $"OnInitializedAsync triggered",
            settingLevel,
            MessageLevel.Information
        );

        filteredUsers = new List<UserAccount>(users);
        PaginationService.ItemsPerPage = 8;

        if (!authUser.IsInRole("Administrator"))
            SelectUser(users[0]);

        await base.OnInitializedAsync();
    }

    protected override async Task OnAfterRenderAsync(bool FirstRender)
    {
        if (FirstRender && urlPage == taskName)
        {
            CommonLib.DisplayConsole(
                taskName,
                $"OnAfterRenderAsync first rendor triggered",
                settingLevel,
                MessageLevel.Information
            );
            try
            {
                registration = NavigationManager.RegisterLocationChangingHandler(OnLocationChanging);

                workerNo = await WorkerService.StartWorker();
                await ScreenWakeLockService.RequestWakeLockAsync();
            }
            catch (Exception ex)
            {
                CommonLib.DisplayConsole(
                taskName,
                ex,
                settingLevel,
                MessageLevel.Error
            );
            }
        }

        await base.OnAfterRenderAsync(FirstRender);
    }

    public void Dispose()
    // Need to add "@implements IDisposable" at the top of the razor file
    // and add ": IDisposable" after partial class on the same line
    {
        CommonLib.DisplayConsole(
            taskName,
            $"Dispose triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            DisposeAsync(true);
            GC.SuppressFinalize(this);
        }
        catch (Exception ex)
        {
            CommonLib.DisplayConsole(
                taskName,
                ex,
                settingLevel,
                MessageLevel.Error
            );
        }
    }

    protected async virtual void DisposeAsync(bool Disposing)
    {
        CommonLib.DisplayConsole(
            taskName,
            $"DisposeAsync triggered",
            settingLevel,
            MessageLevel.Information
        );

        if (!disposed)
        {
            if (Disposing)
            // Cleanup managed resources
            {
                try
                {
                    // Unsubscribe customize LocationChanged event from URL
                    NavigationManager.LocationChanged -= OnLocationChanged;
                    registration?.Dispose();

                    await WorkerService.StopWorker(workerNo);
                    await ScreenWakeLockService.ReleaseWakeLockAsync();

                }
                catch (Exception ex)
                {
                    CommonLib.DisplayConsole(
                        taskName,
                        ex,
                        settingLevel,
                        MessageLevel.Error
                    );
                }
            }

            // Cleanup unmanaged resources (if any)
            disposed = true;
        }
    }

    // Finalizer (destructor)
    ~Users()
    {
        CommonLib.DisplayConsole(
            taskName,
            $"Finalizer triggered",
            settingLevel,
            MessageLevel.Information
        );
        DisposeAsync(false);
    }

    private ValueTask OnLocationChanging(LocationChangingContext context)
    {
        CommonLib.DisplayConsole(
            taskName,
            $"OnLocationChanging triggered",
            settingLevel,
            MessageLevel.Information
        );
        // context.PreventNavigation();
        return ValueTask.CompletedTask;
    }

    private async void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    // Handle the URL change here (e.g., update styles, perform actions, etc.)
    {
        await UpdateInfoChange();
        await InvokeAsync(StateHasChanged);
    }

    private Task UpdateInfoChange()
    // Update all the necessary variable or task for the page when URL change
    {
        CommonLib.DisplayConsole(
            taskName,
            $"UpdateInfoChange triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            currentUrl = NavigationManager.Uri;
            urlPage = CommonLib.GetPageName(currentUrl, 0);

            CommonLib.DisplayConsole(
                taskName,
                $"UpdateInfoChange: urlPage={urlPage}, taskName={taskName}",
                settingLevel,
                MessageLevel.Trace
            );

            if (urlPage != taskName)
                return Task.CompletedTask;

            var columns = new List<string> { "UserName", "FirstName", "LastName", "UserRole", "PageDescription" };
            SortingModel = new SortingModel<UserAccount>(columns);

            CommonLib.DisplayConsole(
                taskName,
                $"UpdateInfoChange done",
                settingLevel,
                MessageLevel.Information
            );
        }
        catch (Exception ex)
        {
            CommonLib.DisplayConsole(
                taskName,
                ex,
                settingLevel,
                MessageLevel.Error
            );
        }

        return Task.CompletedTask;
    }

    public void AddUser()
    {
        NavigationManager.NavigateTo($"/Account/Add/{currentUserID}", true, true);
    }

    private async void EditUser(UserAccount User)
    {
        if (User.IsBusy) return;

        User.IsBusy = true;
        User.IsEdit = true;
        disableAddButton = true;
        await Task.Delay(200);
        User.IsBusy = false;
        User.IsEdit = false;
        disableAddButton = false;
        NavigationManager.NavigateTo($"/Account/Edit/{User.UserId}", true, true);
    }

    private async void DeleteUser(UserAccount User)
    {
        if (User.IsBusy || User.UserName == currentUserName) return;

        User.IsBusy = true;
        User.IsDelete = true;
        disableAddButton = true;
        await Task.Delay(200);
        UserAccountService.DeleteUserName(User);
        users = UserAccountService.GetUserList(authUser);
        page = MattelService.GetPageList();

        foreach (var user in users)
        {
            var findDescription = page.PageTypeList.FirstOrDefault(
                n => n.Type == user.Page
            );

            if (findDescription != null)
                user.PageDescription = findDescription.Description;
            else
                user.PageDescription = string.Empty;
        }

        User.IsBusy = false;
        User.IsDelete = false;
        disableAddButton = false;
        await InvokeAsync(StateHasChanged);
    }

    private async Task UpdateLocationTable()
    {
        CommonLib.DisplayConsole(
           taskName,
           $"UpdateLocationTable triggered",
           settingLevel,
           MessageLevel.Information
        );
        await Task.Delay(1);

        try
        {
            if (users?.Count > 0)
            {
                loading = false;
                CommonLib.DisplayConsole(
                    taskName,
                    $"UpdateInfoChange: urlPage={urlPage}, taskName={taskName}",
                    settingLevel,
                    MessageLevel.Trace
                );
            }

            await InvokeAsync(StateHasChanged);
        }
        catch (Exception ex)
        {
            CommonLib.DisplayConsole(
                taskName,
                ex,
                settingLevel,
                MessageLevel.Error
            );
        }
    }

    private void ShowAddUserModal()
    {
        showModal = true;
        modalContent = "add";
    }

    private void ShowAddModalChanged(bool value)
    {
        showModal = value;
    }

    private async Task OnUserAdded(UserAccount User)
    {
        if (User != null)
        {
            User.IsChecking = true;
            UserAccount? existingUser = users.FirstOrDefault(
                c => string.Equals(
                    c.UserName,
                    User.UserName,
                    StringComparison.OrdinalIgnoreCase
                )
                && c.UserId != User.UserId
            );

            if (existingUser != null)
            {
                popMsgData.ActivatePopup = true;
                popMsgData.Text_1 = "The username already exists.";
                popMsgData.Text_2 = "Please use a different username.";
                popMsgData.Middle_Icon = "icon_issue";
                popMsgData.BtnText_1 = "OK";
                popMsgData.BtnClick_1 = async () => await CloseAddUserModal();
                popMsgData.BtnText_2 = null!;
                popMsgData.BtnClick_2 = null!;
                User.IsChecking = false;
                await InvokeAsync(StateHasChanged);
                return;
            }

            UserAccountService.AddUser(User);
            User.IsChecking = false;

            popMsgData.ActivatePopup = true;
            popMsgData.Text_1 = $"New User created";
            popMsgData.Text_2 = "successfully !";
            popMsgData.Middle_Icon = "icon_add_user";
            popMsgData.BtnText_1 = "OK";
            popMsgData.BtnClick_1 = () => Confirm();
            popMsgData.BtnText_2 = null!;
            popMsgData.BtnClick_2 = null!;

            await InvokeAsync(StateHasChanged);
            showModal = false;
        }
    }

    private void Confirm()
    {
        NavigationManager.NavigateTo("/Account/Users", true, true);
    }

    private async Task CloseAddUserModal()
    {
        popMsgData.ActivatePopup = false;
        await InvokeAsync(StateHasChanged);
        modalContent = "add";
        showModal = true;
    }

    private async Task EditSelectedUser()
    {
        if (selectedUser != null)
        {
            ShowEditUserModal(selectedUser.UserId);
            await ShowEditModalChanged(true);
        }
    }

    private void ShowEditUserModal(int userId)
    {
        selectedUserId = userId; //to call data
        showModal = true;
        modalContent = "edit";
    }

    private async Task ShowEditModalChanged(bool value)
    {
        showModal = value;
        await InvokeAsync(StateHasChanged);
    }

    private async Task OnUserEdited(UserAccount User)
    {
        if (User != null)
        {
            // Find the part being edited by its ID
            var currentUser = users.FirstOrDefault(p => p.UserId == User.UserId);

            if (currentUser != null)
            {
                User.IsChecking = true;

                //update user detail
                currentUser.FirstName = User.FirstName;
                currentUser.LastName = User.LastName;
                currentUser.Password = User.Password;
                currentUser.UserRole = User.UserRole;
                currentUser.Page = User.Page;

                // Set IsChecking to false after updates 
                user.IsChecking = false;

                //Save new details
                UserAccountService.SaveUserInfo(User);

                popMsgData.ActivatePopup = true;
                popMsgData.Text_1 = $"User updated";
                popMsgData.Text_2 = "successfully !";
                popMsgData.Middle_Icon = "icon_mobile_check";
                popMsgData.BtnText_1 = "OK";
                popMsgData.BtnClick_1 = () => ConfirmUpdate();
                popMsgData.BtnText_2 = null!;
                popMsgData.BtnClick_2 = null!;

                // Update the user list for display 
                await InvokeAsync(StateHasChanged);

                // Close the modal after editing 
                await ShowEditModalChanged(false);
            }
        }
    }

    private void ConfirmUpdate()
    {
        NavigationManager.NavigateTo("/Account/Users", true, true);
    }

    private void DeleteSelectedUser()
    {
        if (selectedUser != null)
        {
            ShowDeleteUserModal(selectedUser.UserId);
        }
    }

    private async void ShowDeleteUserModal(int userId)
    {
        selectedUserId = userId;
        popMsgData.ActivatePopup = true;
        popMsgData.Text_1 = $"Are you sure you want to";
        popMsgData.Text_2 = $"delete User \"{selectedUser.UserName}\" ?";
        popMsgData.Middle_Icon = "icon_remove_friend";
        popMsgData.BtnText_1 = "OK";
        popMsgData.BtnClick_1 = async () => await ConfirmRemove();
        popMsgData.BtnText_2 = "Cancel";
        popMsgData.BtnClick_2 = () => CloseRemoveModal();
        await InvokeAsync(StateHasChanged);
    }

    private async void CloseRemoveModal()
    {
        popMsgData.ActivatePopup = false;
        showModal = false;
        await InvokeAsync(StateHasChanged);
    }

    private async Task ConfirmRemove()
    {
        var user = UserAccountService.GetUserList(authUser).FirstOrDefault(u => u.UserId == selectedUserId);
        if (user != null && user.UserName != currentUserName)
        {
            UserAccountService.DeleteUserName(user);
            NavigationManager.NavigateTo("/Account/Users", true, true);
        }

        popMsgData.ActivatePopup = false;
        showModal = false;
        await InvokeAsync(StateHasChanged);
    }

    private async void ShowSuccessModal(string content)
    {
        showSuccessModal = true;
        successContent = content;
        await InvokeAsync(StateHasChanged);
    }

    private async void ShowSuccessModalChanged(bool value)
    {
        showSuccessModal = value;
        await InvokeAsync(StateHasChanged);
    }

    private void SelectUser(UserAccount user)
    {
        selectedUser = user;
    }

    private string GetRowClass(UserAccount user)
    {
        return
            (
                selectedUser != null
                && selectedUser.UserId == user.UserId
            )
            ? "row-active"
            : string.Empty;
    }

    private async void FilterUsers(string filter)
    {
        selectedUser = null!;

        if (string.IsNullOrWhiteSpace(filter))
        {
            filteredUsers = new List<UserAccount>(users);
        }
        else
        {
            filteredUsers = users.Where(u =>
                u.UserName.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                u.FirstName.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                u.LastName.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                u.UserRole.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                u.PageDescription.Contains(filter, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        PaginationService.SetItems(filteredUsers);
        PaginationService.CurrentPage = 1;
        await InvokeAsync(StateHasChanged);
    }

    private async void Sort()
    {
        CommonLib.DisplayConsole(
            taskName,
            $"Sort triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            // Set initial sorting for the first column (UserName) 
            SortingModel.SortStates["UserName"] = SortState.Descending;
            SortingModel.SortIcons["UserName"] = "icon_sortAsc_darkGrey";

            // Perform initial sort
            users = await SortingModel.SortItemsAsync(users, "UserName");

            // Ensure pagination service is updated with sorted users
            PaginationService.SetItems(users);
            PaginationService.GoToPage(1);
        }
        catch (Exception ex)
        {
            CommonLib.DisplayConsole(
                taskName,
                ex,
                settingLevel,
                MessageLevel.Error
            );
        }
    }

    private async Task SortItems(string column)
    {
        CommonLib.DisplayConsole(
            taskName,
            $"SortItems triggered",
            settingLevel,
            MessageLevel.Information
        );

        try
        {
            users = await SortingModel.SortItemsAsync(users, column);
            PaginationService.SetItems(users); // Reset pagination
            await InvokeAsync(StateHasChanged);
        }
        catch (Exception ex)
        {
            CommonLib.DisplayConsole(
                taskName,
                ex,
                settingLevel,
                MessageLevel.Error
            );
        }
    }

    private async void OnPageChanged()
    {
        selectedUser = null!;
        await InvokeAsync(StateHasChanged);
    }
}