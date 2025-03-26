using AppGestionStock.Data;
using AppGestionStock.Middlewares;
using AppGestionStock.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();

builder.Services.AddTransient<RepositoryAlmacen>();
string connectionString = builder.Configuration.GetConnectionString("SqlAzure");
//builder.Services.AddDbContext<AlmacenesContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddDbContext<AlmacenesContext>(options => options.UseSqlServer(connectionString), ServiceLifetime.Scoped);

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

app.UseAuthorization();

app.UseSession();
app.UseMiddleware<AuthenticationMiddleware>();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();