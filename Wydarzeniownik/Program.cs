using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Wydarzeniownik.Data;

var builder = WebApplication.CreateBuilder(args);

// Dodajemy DbContext i Identity
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseInMemoryDatabase("EventDb"));

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

builder.Services.AddRazorPages();

var app = builder.Build();

// Konfiguracja œcie¿ek
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
