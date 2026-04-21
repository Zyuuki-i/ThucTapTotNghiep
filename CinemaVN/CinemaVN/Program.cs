using CinemaVN.MyModels;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddSingleton<EmailService>();

builder.Services.AddHttpContextAccessor();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(10);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.Use(async (context, next) =>
{
    var path = context.Request.Path.ToString().ToLower();

    if (path.StartsWith("/admin"))
    {
        var role = context.Session.GetString("UserRole");

        if (string.IsNullOrEmpty(role) || role != "admin")
        {
            context.Response.Redirect("/NguoiDung/DangNhap");
            return;
        }
    }

    if (path.StartsWith("/manage"))
    {
        var role = context.Session.GetString("UserRole");

        if (string.IsNullOrEmpty(role) || role != "manage")
        {
            context.Response.Redirect("/NguoiDung/DangNhap");
            return;
        }
    }

    if (path.StartsWith("/staff"))
    {
        var role = context.Session.GetString("UserRole");

        if (string.IsNullOrEmpty(role) || role != "staff")
        {
            context.Response.Redirect("/NguoiDung/DangNhap");
            return;
        }
    }

    await next();
});

app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
