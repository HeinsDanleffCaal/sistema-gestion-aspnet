using Microsoft.AspNetCore.Authentication.Cookies;
using SistemaGestion.Data;
using SistemaGestion.Services;
using SistemaGestion.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// MVC
builder.Services.AddControllersWithViews();

// Acceso a datos (Dapper)
builder.Services.AddScoped<IDbConnectionFactory, SqlConnectionFactory>();

// Servicios de negocio
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IProductoService, ProductoService>();
builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddScoped<IPedidoService, PedidoService>();

// Hasher de contraseñas (no depende de EF/Identity, sirve con Dapper)
builder.Services.AddScoped<Microsoft.AspNetCore.Identity.IPasswordHasher<SistemaGestion.Models.Usuario>,
    Microsoft.AspNetCore.Identity.PasswordHasher<SistemaGestion.Models.Usuario>>();

// Autenticación por cookies + autorización por roles
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.Name = "SistemaGestion.Auth";
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SoloAdministrador", policy => policy.RequireRole("Administrador"));
});

var app = builder.Build();

// -----------------------------------------------------------------------
// Seed inicial: si todavía no existe ningún usuario Administrador,
// se crea uno por defecto para poder ingresar la primera vez.
// Usuario: admin   Contraseña: Admin123$
// IMPORTANTE: cambia esta contraseña (o crea otro admin y elimina este)
// apenas entres por primera vez a producción.
// -----------------------------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    var usuarioService = scope.ServiceProvider.GetRequiredService<IUsuarioService>();
    var passwordHasher = scope.ServiceProvider.GetRequiredService<
        Microsoft.AspNetCore.Identity.IPasswordHasher<SistemaGestion.Models.Usuario>>();

    var admin = await usuarioService.ObtenerPorNombreUsuarioAsync("admin");
    if (admin is null)
    {
        var nuevoAdmin = new SistemaGestion.Models.Usuario
        {
            NombreUsuario = "admin",
            NombreCompleto = "Administrador del Sistema",
            Email = "admin@sistemagestion.local",
        };
        nuevoAdmin.PasswordHash = passwordHasher.HashPassword(nuevoAdmin, "Admin123$");

        // RolId = 1 corresponde a 'Administrador' según el seed de 01_Schema.sql
        await usuarioService.CrearAsync(nuevoAdmin, rolId: 1);
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
