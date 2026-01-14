using COEM.LicenseIQ.WebUI.Components;
using Azure.Identity;
using COEM.LicenseIQ.Application;
using COEM.LicenseIQ.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

var keyVaultUrl = builder.Configuration["KeyVault:BaseUrl"];

if (!string.IsNullOrEmpty(keyVaultUrl))
{
    // DefaultAzureCredential intentará autenticarse en este orden:
    // 1. Variables de Entorno
    // 2. Visual Studio / VS Code (Tu cuenta de desarrollador)
    // 3. Azure CLI (az login)
    // 4. Managed Identity (Cuando esté publicado en Azure)
    builder.Configuration.AddAzureKeyVault(
        new Uri(keyVaultUrl),
        new DefaultAzureCredential());
}

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
