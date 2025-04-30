using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.OpenApi.Models;
using Prometheus;
using Serilog;
using Serilog.Events;
using ServerOverflow.Backend.Services;
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
builder.Services.AddSerilog();
builder.Services.AddSwaggerGen(c => {
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ServerOverflow API", Version = "v1" });
    c.AddSecurityDefinition("bearerAuth", new OpenApiSecurityScheme {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "Custom",
        Description = "Authorization header using the Bearer scheme"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement { {
        new OpenApiSecurityScheme {
            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "bearerAuth" }
        }, [] }});
});

var app = builder.Build();
if (!app.Environment.IsDevelopment()) {
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapMetrics();
app.UseSwagger();
app.UseSwaggerUI(c => {
    c.SwaggerEndpoint("v1/swagger.json", "ServerOverflow API");
});

Log.Information("Website is now running");
app.Run();