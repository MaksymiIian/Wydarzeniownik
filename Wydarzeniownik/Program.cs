using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Wydarzeniownik.Data;

var builder = WebApplication.CreateBuilder(args);
//var connectionString = builder.Configuration.GetConnectionString("ApplicationDbContextConnection") ?? throw new InvalidOperationException("Connection string 'ApplicationDbContextConnection' not found.");
/*
// Dodajemy DbContext i Identity
builder.Services.AddDbContext<ApplicationDbContext>(options =>
     options.UseInMemoryDatabase("EventDb"));
*/
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

builder.Services.AddRazorPages();

var app = builder.Build();

// Konfiguracja ścieżek
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// Zmieniamy routing na nowy
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Events}/{action=EventsViewer}/{id?}");

// Mapowanie Razor Pages
app.MapRazorPages();

app.Run();
