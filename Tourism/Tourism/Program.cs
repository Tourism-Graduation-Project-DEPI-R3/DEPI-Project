using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Tourism.Models;
using Tourism.IRepository;
using Tourism.Repository;

var builder = WebApplication.CreateBuilder(args);

// ===== Add Controllers with Views =====
builder.Services.AddControllersWithViews();

// ===== Configure Session =====
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ===== Configure DbContext =====
builder.Services.AddDbContext<TourismDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection") ??
                         "Data Source=.;Initial Catalog=Tourism;Integrated Security=True;Encrypt=False;Trust Server Certificate=True"));

// ===== Authentication & Authorization =====
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Home/ChooseLogin";
        options.AccessDeniedPath = "/Home/index";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
    });

builder.Services.AddAuthorization();

// ===== Dependency Injection =====

// Generic repository
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Custom Repositories
builder.Services.AddScoped<ITouristRepository, TouristRepository>();
builder.Services.AddScoped<IMerchantRepository, MerchantRepository>();
builder.Services.AddScoped<IAdminRepository, AdminRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IRestaurantRepository, RestaurantRepository>();
builder.Services.AddScoped<ICreditCardRepository, CreditCardRepository>();
builder.Services.AddScoped<IMealRepository, MealRepository>();
builder.Services.AddScoped<ITableRepository, TableRepository>();
builder.Services.AddScoped<IRoomRepository, RoomRepository>();
builder.Services.AddScoped<IHotelRepository, HotelRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Trip booking repositories
builder.Services.AddScoped<ITripRepository, TripRepository>();
builder.Services.AddScoped<ITourGuideRepository, TourGuideRepository>();

var app = builder.Build();

// ===== Middleware =====
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

// ===== Routes =====
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
