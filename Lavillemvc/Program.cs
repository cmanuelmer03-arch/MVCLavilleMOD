
using Lavillemvc.DAL;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. CONFIGURACIÓN MVC
// ==========================================

builder.Services.AddControllersWithViews();

// ==========================================
// 2. CONEXIÓN A SUPABASE
// ==========================================

// Conexión existente de tu equipo
builder.Services.AddScoped<Conexion>();

// Repositorio para consultar cuentas
builder.Services.AddScoped<CuentaRepositorio>();

// ==========================================
// 3. SESIONES EXISTENTES DEL PROYECTO
// ==========================================

// Conservamos la configuración de tu equipo
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(45);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

// ==========================================
// 4. AUTENTICACIÓN POR COOKIES
// ==========================================

builder.Services
    .AddAuthentication(
        CookieAuthenticationDefaults.AuthenticationScheme
    )
    .AddCookie(options =>
    {
        options.LoginPath = "/Login/Index";
        options.AccessDeniedPath = "/Login/AccesoDenegado";

        options.Cookie.Name = "LAvielle.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;

        options.ExpireTimeSpan = TimeSpan.FromMinutes(45);
        options.SlidingExpiration = true;
    });

// ==========================================
// 5. AUTORIZACIÓN POR ROLES
// ==========================================

builder.Services.AddAuthorization();

// ==========================================
// 6. CONSTRUCCIÓN DE LA APLICACIÓN
// ==========================================

var app = builder.Build();

// ==========================================
// 7. CONFIGURACIÓN DE ERRORES
// ==========================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// ==========================================
// 8. MIDDLEWARE
// ==========================================

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

// Sesiones existentes del proyecto
app.UseSession();

// Autenticación de usuarios
app.UseAuthentication();

// Autorización y permisos
app.UseAuthorization();

// ==========================================
// 9. RUTA PRINCIPAL
// ==========================================

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");

// ==========================================
// 10. INICIAR APLICACIÓN
// ==========================================

app.Run();
