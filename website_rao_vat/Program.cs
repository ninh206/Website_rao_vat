using Microsoft.EntityFrameworkCore;
using website_rao_vat.Data; // Đảm bảo đúng namespace của thư mục Data

var builder = WebApplication.CreateBuilder(args);

// --- 1. CẤU HÌNH SERVICES (Add Services) ---

builder.Services.AddControllersWithViews();

// Cấu hình Database
builder.Services.AddDbContext<DataBaseWebRaoVatContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Cấu hình Session (Để lưu thông tin đăng nhập)
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Hết hạn sau 30 phút
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Cấu hình IHttpContextAccessor (Để đọc Session trong file Layout)
builder.Services.AddHttpContextAccessor();


var app = builder.Build();

// --- 2. CẤU HÌNH PIPELINE (Use Middleware) ---

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // Thay cho MapStaticAssets nếu bạn dùng bản cũ, hoặc giữ cả hai

app.UseRouting();

// KÍCH HOẠT SESSION (Phải đặt trước Authorization)
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();