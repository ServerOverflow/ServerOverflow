using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning) 
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .WriteTo.Console().CreateLogger();

Log.Information("Starting ServerOverflow");

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddSerilog();

var app = builder.Build();
if (!app.Environment.IsDevelopment()) {
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseStatusCodePagesWithReExecute("/error");
app.MapControllerRoute(name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

Log.Information("Website is now running");
app.Run();