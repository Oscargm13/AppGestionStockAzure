using AppGestionStock.Data;
using AppGestionStock.Middlewares;
using AppGestionStock.Repositories;
using AppGestionStock.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
});
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
}).AddCookie();

builder.Services.AddTransient<RepositoryAlmacen>();
builder.Services.AddTransient<ServiceAlmacenes>();
string connectionString = builder.Configuration.GetConnectionString("SqlAzure");
//builder.Services.AddDbContext<AlmacenesContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddDbContext<AlmacenesContext>(options => options.UseSqlServer(connectionString), ServiceLifetime.Scoped);
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
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

app.UseSession();
app.UseMiddleware<AuthenticationMiddleware>();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();