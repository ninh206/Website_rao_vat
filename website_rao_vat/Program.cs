using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using website_rao_vat.Data;

var builder = WebApplication.CreateBuilder(args);

// --- 1. CẤU HÌNH SERVICES ---

builder.Services.AddControllersWithViews();

// Cấu hình Database
builder.Services.AddDbContext<DataBaseWebRaoVatContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Cấu hình Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddHttpContextAccessor();

// Cấu hình xác thực bằng Cookie (ĐÃ GỘP VÀO CHO GỌN)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login"; // Đảm bảo đúng đường dẫn trang Login của ông
        options.AccessDeniedPath = "/Account/AccessDenied";
    });

var app = builder.Build();

// --- 2. CẤU HÌNH PIPELINE (THỨ TỰ CỰC KỲ QUAN TRỌNG) ---

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 1. Chạy Session
app.UseSession();

// 2. Chạy Xác thực (DÒNG NÀY ÔNG ĐANG THIẾU NÈ!)
app.UseAuthentication();

// 3. Chạy Phân quyền (Phải đứng sau Authentication)
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();