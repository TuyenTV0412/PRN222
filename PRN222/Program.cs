using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PRN222.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Cấu hình DbContext với SQL Server
builder.Services.AddDbContext<Prn222Context>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Cấu hình Session
builder.Services.AddDistributedMemoryCache(); // Sử dụng bộ nhớ để lưu trữ session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Thời gian hết hạn session
    options.Cookie.HttpOnly = true; // Chỉ có thể truy cập cookie từ phía server
    options.Cookie.IsEssential = true; // Đảm bảo cookie luôn được gửi
});


builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

// Cấu hình Authentication với Google
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
.AddCookie() // Sử dụng Cookie Authentication
.AddGoogle(options =>
{
    options.ClientId = "745365451388-5bb4o66pmqb9rqrd2vdufr5p0gsk7ep4.apps.googleusercontent.com";
    options.ClientSecret = "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";
});

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

// Cấu hình Authentication & Authorization Middleware
app.UseAuthentication();  // Thêm dòng này để kích hoạt Authentication
app.UseAuthorization();
app.UseSession(); // Đảm bảo gọi UseSession sau UseRouting và trước UseAuthorization

app.UseStaticFiles();


// Cấu hình Session middleware
app.UseSession(); // Đảm bảo gọi UseSession sau UseRouting và trước UseAuthorization

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
