using Microsoft.EntityFrameworkCore;
using ElectroGestion.Components;
using ElectroGestion.Context;
using ElectroGestion.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var ConStr = builder.Configuration.GetConnectionString("SqlConStr");

builder.Services.AddDbContextFactory<Contexto>(options =>
    options.UseSqlServer(ConStr));

builder.Services.AddScoped<ModelosService>();
builder.Services.AddScoped<AppStateService>();
builder.Services.AddScoped<ClienteService>();
builder.Services.AddScoped<ServicioService>();
builder.Services.AddScoped<SolicitudService>();

builder.Services.AddBlazorBootstrap();

var app = builder.Build();

if (args.Contains("--test-crud"))
{
    using var scope = app.Services.CreateScope();
    var cliService = scope.ServiceProvider.GetRequiredService<ClienteService>();
    var srvService = scope.ServiceProvider.GetRequiredService<ServicioService>();
    var solService = scope.ServiceProvider.GetRequiredService<SolicitudService>();
    var exito = await CrudTester.RunAsync(cliService, srvService, solService);
    Environment.Exit(exito ? 0 : 1);
    return;
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.UseHttpsRedirection();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

