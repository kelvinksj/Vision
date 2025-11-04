using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using TaskManagerWeb.Components;
using TaskManagerWeb.Components.Helper;
using TaskManagerWeb.Components.Models.Common;
using TaskManagerWeb.Components.Models.Http;
using TaskManagerWeb.Components.Service.Authentication;
using TaskManagerWeb.Components.Service.DR;
using TaskManagerWeb.Components.Service.Info;
using TaskManagerWeb.Components.Service.KeepAlive;
using TaskManagerWeb.Components.Service.Mattel;
using TaskManagerWeb.Components.Service.Page;
using TaskManagerWeb.Components.Service.RCS;
using TaskManagerWeb.Components.Service.ScreenWake;
using TaskManagerWeb.Components.Service.Setting;
using TaskManagerWeb.Components.Service.Site;

WebApplicationBuilder? builder = WebApplication.CreateBuilder(args);

// Ensure the app runs as a systemd service on Linux
//builder.Host.UseSystemd();

// Ensure the app runs as a Windows Service on Windows
//builder.Host.UseWindowsService();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddServerSideBlazor(options =>
                 {
                     options.DetailedErrors = true;
                     options.DisconnectedCircuitMaxRetained = 1000;
                     options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(3);
                     options.JSInteropDefaultCallTimeout = TimeSpan.FromMinutes(1);
                     options.MaxBufferedUnacknowledgedRenderBatches = 10;
                 });

// Add authentication service.
builder.Services.AddAuthenticationCore();

// Add HttpClient using IHttpClientFactory service.
builder.Services.AddHttpClient("KeepAliveClient");

// Add for data encryption and decryption.
builder.Services.AddDataProtection();

// Add controllers.
builder.Services.AddControllers();

// Add services to the project.
builder.Services.AddTransient(typeof(PaginationService<>));
builder.Services.AddTransient(typeof(SortingModel<>));

builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
builder.Services.AddScoped<HttpClass>();
builder.Services.AddScoped<ProtectedSessionStorage>();
builder.Services.AddScoped<SiteExitService>();

builder.Services.AddSingleton<CommonLib>();
builder.Services.AddSingleton<DirectoryService>();
builder.Services.AddSingleton<MattelService>();
builder.Services.AddSingleton<MessageService>();
builder.Services.AddSingleton<PrintService>();
builder.Services.AddSingleton<ScreenWakeLockService>();
builder.Services.AddSingleton<SettingsService>();
builder.Services.AddSingleton<UserAccountService>();
builder.Services.AddSingleton<WorkerService>();

builder.Services.AddSingleton<ContainerService>();
builder.Services.AddHostedService<ContainerService>(
    sp => sp.GetService<ContainerService>()!
);

builder.Services.AddSingleton<ScanService>();
builder.Services.AddHostedService<ScanService>(
    sp => sp.GetService<ScanService>()!
);

builder.Services.AddSingleton<TaskManagerService>();
builder.Services.AddHostedService<TaskManagerService>(
    sp => sp.GetService<TaskManagerService>()!
);

builder.Services.AddSingleton<KeepAliveService>();
builder.Services.AddHostedService<KeepAliveService>(
    sp => sp.GetRequiredService<KeepAliveService>()
);

builder.Services.AddSingleton<RCSService>();
builder.Services.AddHostedService<RCSService>(
    sp => sp.GetRequiredService<RCSService>()
);

WebApplication? app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    //The default HSTS value is 30 days.You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
   .AddInteractiveServerRenderMode();

// Add this line to map controller routes
app.MapControllers();

app.Run();