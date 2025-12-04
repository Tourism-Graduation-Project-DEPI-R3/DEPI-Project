using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Tourism.Models;
using Tourism.IRepository;
using Tourism.Repository;

var builder = WebApplication.CreateBuilder(args);

// ================= Controllers =================
builder.Services.AddControllersWithViews();

// ================= Session =================
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ================= Authentication & Authorization =================
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Tourism/Login";
        options.AccessDeniedPath = "/Home/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
    });

builder.Services.AddAuthorization();

// ================= DbContext (MySQL) =================
builder.Services.AddDbContext<TourismDbContext>(options =>
{
    var con = builder.Configuration.GetConnectionString("Con");
    options.UseMySql(con, ServerVersion.AutoDetect(con));
});

// ================= Dependency Injection =================
// Generic repository
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Custom Repositories
builder.Services.AddScoped<ITouristRepository, TouristRepository>();   // includes Cart methods (AddToCart, ChangeQuantity, etc.)
builder.Services.AddScoped<IMerchantRepository, MerchantRepository>();
builder.Services.AddScoped<IAdminRepository, AdminRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ITourGuideRepository, TourGuideRepository>();

// Trip repository (used where needed)
builder.Services.AddScoped<ITripRepository, TripRepository>();

// Tourism repository (for TourismController)
builder.Services.AddScoped<ITourismRepository, TourismRepository>();

var app = builder.Build();

// ================= Seed Default Admin =================
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<TourismDbContext>();

    if (!context.Admins.Any())
    {
        var admin = new Admin { email = "admin@tourism.com" };
        var hasher = new PasswordHasher<Admin>();
        admin.passwordHash = hasher.HashPassword(admin, "admin123");

        context.Admins.Add(admin);
        context.SaveChanges();
    }
}

// ================= Middleware =================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession(); // enable session

app.UseAuthentication();
app.UseAuthorization();

// ================= Routes =================
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Tourism}/{action=Index}/{id?}");

app.Run();
