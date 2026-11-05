using Microsoft.EntityFrameworkCore;
using ElectroGestion.Components;
using ElectroGestion.Context;
using ElectroGestion.Models;
using ElectroGestion.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
{
    options.LoginPath = "/login";
    options.Cookie.Name = "ElectroGestion.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.SlidingExpiration = true;
    options.Events.OnValidatePrincipal = async context =>
    {
        if (!int.TryParse(context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier), out var id)) { context.RejectPrincipal(); return; }
        var auth = context.HttpContext.RequestServices.GetRequiredService<AutenticacionService>();
        if (await auth.ObtenerUsuarioAsync(id) is null) context.RejectPrincipal();
    };
});
builder.Services.AddAuthorization();
builder.Services.Configure<ForwardedHeadersOptions>(options => options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto);
var keysPath = builder.Configuration["DataProtection:KeysPath"]
    ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ElectroGestion", "keys");
Directory.CreateDirectory(keysPath);
builder.Services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo(keysPath));

var ConStr = builder.Configuration.GetConnectionString("SqlConStr");

builder.Services.AddDbContextFactory<Contexto>(options =>
    options.UseSqlServer(ConStr));

builder.Services.AddScoped<ModelosService>();
builder.Services.AddScoped<AppStateService>();
builder.Services.AddScoped<AutenticacionService>();
builder.Services.AddScoped<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();
builder.Services.AddScoped<ClienteService>();
builder.Services.AddScoped<ServicioService>();
builder.Services.AddScoped<InventarioService>();
builder.Services.AddScoped<VentaService>();
builder.Services.AddScoped<SolicitudService>();
builder.Services.AddScoped<SolicitudEventoService>();
builder.Services.AddScoped<VisitaService>();
builder.Services.AddScoped<LevantamientoService>();
builder.Services.AddScoped<CotizacionService>();
builder.Services.AddScoped<OrdenTrabajoService>();
builder.Services.AddScoped<CobroService>();
builder.Services.AddScoped<InicioDashboardService>();
builder.Services.AddScoped<ReporteConsumoService>();

builder.Services.AddBlazorBootstrap();

var app = builder.Build();
app.UseForwardedHeaders();

if (args.Contains("--test-crud"))
{
    using var scope = app.Services.CreateScope();
    var cliService = scope.ServiceProvider.GetRequiredService<ClienteService>();
    var srvService = scope.ServiceProvider.GetRequiredService<ServicioService>();
    var solService = scope.ServiceProvider.GetRequiredService<SolicitudService>();
    var visService = scope.ServiceProvider.GetRequiredService<VisitaService>();
    var exito = await CrudTester.RunAsync(cliService, srvService, solService, visService);
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
app.UseAuthentication();
app.UseAuthorization();

app.Use(async (context, next) =>
{
    var ruta = context.Request.Path.Value ?? "/";
    if (context.Request.Method == "GET" && !ruta.StartsWith("/_", StringComparison.OrdinalIgnoreCase)
        && string.IsNullOrEmpty(Path.GetExtension(ruta))
        && context.Request.Path != "/login" && context.Request.Path != "/not-found" && context.Request.Path != "/Error"
        && context.User.Identity?.IsAuthenticated != true)
    {
        context.Response.Redirect("/login");
        return;
    }
    await next();
});

app.UseAntiforgery();

app.MapPost("/auth/login", async (HttpContext context, AutenticacionService auth) =>
{
    if (!context.Request.Headers.TryGetValue("Origin", out var origin) ||
        !Uri.TryCreate(origin.ToString(), UriKind.Absolute, out var origen) ||
        !string.Equals(origen.Authority, context.Request.Host.Value, StringComparison.OrdinalIgnoreCase))
        return Results.BadRequest();
    var datos = await context.Request.ReadFromJsonAsync<LoginRequest>();
    if (datos is null || string.IsNullOrWhiteSpace(datos.Usuario) || string.IsNullOrEmpty(datos.Password)) return Results.BadRequest();
    var usuario = await auth.AutenticarAsync(datos.Usuario.Trim(), datos.Password);
    if (usuario is null) return Results.Unauthorized();
    var claims = new[] { new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()), new Claim(ClaimTypes.Name, usuario.Nombre), new Claim(ClaimTypes.Role, usuario.Rol) };
    var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
    await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties { IsPersistent = false });
    return Results.Ok(new { destino = usuario.Rol == "Admin" ? "/recuperacion" : "/" });
});
app.MapPost("/auth/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Ok();
});
app.MapPost("/auth/activity", (HttpContext context) => context.User.Identity?.IsAuthenticated == true ? Results.Ok() : Results.Unauthorized());

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

record LoginRequest(string Usuario, string Password);
