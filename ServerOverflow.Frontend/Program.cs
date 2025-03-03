using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Prometheus;
using Serilog;
using Serilog.Events;
using ServerOverflow.Frontend.Services;
using ServerOverflow.Shared;
using ServerOverflow.Shared.Storage;

Log.Logger = new LoggerConfiguration().MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .WriteTo.Console()
    .CreateLogger();

Log.Information("Starting ServerOverflow Frontend");
Metrics.SuppressDefaultMetrics();

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHostedService<Profiles>();
builder.Services.AddHostedService<Statistics>();
builder.Configuration.AddJsonFile("config.json", optional: true);
Database.Initialize(builder.Configuration["mongo-uri"] ?? "mongodb://127.0.0.1:27017?maxPoolSize=5000");
var accounts = await Database.Accounts.EstimatedCount();
var invites = await Database.Invitations.EstimatedCount();
if (accounts == 0 && invites == 0) {
    var invite = await Invitation.Create("Administrator");
    Log.Warning("There aren't any accounts or invitations!");
    Log.Warning("Use this code: {0}", invite.Code);
}

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options => {
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
    });
builder.Services.AddControllers()
    .AddJsonOptions(options => {
        options.JsonSerializerOptions.IncludeFields = true;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
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
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
app.UseEndpoints(x => x.MapMetrics());

Log.Information("Website is now running");
app.Run();