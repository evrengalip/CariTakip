
using Business.Abstract;
using Business.Concrete;
using CariTakip.Services;
using Data.Abstract;
using Data.Concrete;
using Data.Context;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using PdfSharp.Charting;

var builder = WebApplication.CreateBuilder(args);

ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

// Add services to the container.
builder.Services.AddControllersWithViews();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<DatabaseContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<DatabaseContext>();
builder.Services.AddScoped<ChangeLogEmployeeService>(); // ChangeLogService'i ekle
builder.Services.AddScoped<ChangeLogItemService>();
builder.Services.AddScoped<ChangeLogEmployeeItemService>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IItemRepository, ItemRepository>();
builder.Services.AddHttpContextAccessor(); // IHttpContextAccessor'ý ekle
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IItemService, ItemService>();











builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(opts =>
{
    opts.Cookie.Name = ".CariTakip.auth";
    opts.ExpireTimeSpan = TimeSpan.FromDays(7);
    opts.LoginPath = "/User/Login";
    opts.LogoutPath = "/User/Logout";
    opts.AccessDeniedPath = "/Home/AccessDenied";
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
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
