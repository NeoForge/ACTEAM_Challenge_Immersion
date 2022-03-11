using GestionStock.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<GestionStock.Data.ACTEAM_StockContext>(
    options =>
    {
        options.UseSqlServer(builder.Configuration.GetConnectionString("ACTEAM_StockDB"));
    });

builder.Services.AddAuthentication("AuthCookie").AddCookie("AuthCookie", options =>
{
    options.Cookie.Name = "AuthCookie";
    options.LoginPath = "/Users/Login"; 
    options.AccessDeniedPath = "/Users/AccessDenied"; 
    options.ExpireTimeSpan = TimeSpan.FromDays(7); 
});
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireClaim("Administrator")); 
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
    pattern: "{controller=Items}/{action=Index}/{id?}");

app.Run();
