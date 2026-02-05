using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using PharmacyStock.Infrastructure;
using Serilog;
using Microsoft.EntityFrameworkCore;
using PharmacyStock.Application;
using Microsoft.OpenApi;

// Configure Serilog early to catch startup errors
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting web application");

    var builder = WebApplication.CreateBuilder(args);

    // Use Serilog for all application logging
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

    // Add services to the container.
    builder.Services.AddControllers();

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAngular",
            policy =>
            {
                // Read client app URL from configuration
                var clientAppUrl = builder.Configuration["ClientAppUrl"]
                    ?? "http://localhost:4200";

                policy.WithOrigins(clientAppUrl)
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
    });

    // Add SignalR
    builder.Services.AddSignalR();

    // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
    // builder.Services.AddOpenApi(); // Generates OpenAPI JSON
    // Swagger UI
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Pharmacy Stock API",
            Version = "v1"
        });

        // Note: Bearer OpenApiSecurityScheme is unnecessary because the API uses HTTP-only cookies for authentication.
        // options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
        // {
        //     Type = SecuritySchemeType.Http,
        //     Scheme = "bearer",
        //     BearerFormat = "JWT",
        //     Description = "JWT Authorization header using the Bearer scheme."
        // });
        // options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
        // {
        //     [new OpenApiSecuritySchemeReference("bearer", document)] = []
        // });
    });

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidAudience = builder.Configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
            };
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    context.Token = context.Request.Cookies["accessToken"];
                    return Task.CompletedTask;
                }
            };
        });

    builder.Services.AddInfrastructureServices(builder.Configuration);
    builder.Services.AddApplicationServices();

    // Add HttpContextAccessor for accessing current user
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<PharmacyStock.Application.Interfaces.ICurrentUserService, PharmacyStock.API.Services.CurrentUserService>();

    // Add Dashboard Broadcaster for SignalR (API layer service)
    builder.Services.AddSingleton<PharmacyStock.Application.Interfaces.IDashboardBroadcaster, PharmacyStock.API.Services.DashboardBroadcaster>();

    // Background Services
    builder.Services.AddHostedService<PharmacyStock.API.Services.ScheduledBackgroundService>();


    // Use AddAuthorizationBuilder to register services and policies
    var authorizationBuilder = builder.Services.AddAuthorizationBuilder();

    // Dynamically register policies for all permissions
    var permissionType = typeof(PharmacyStock.Domain.Constants.PermissionConstants);
    var permissionFields = permissionType.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.FlattenHierarchy);

    foreach (var field in permissionFields)
    {
        if (field.IsLiteral && !field.IsInitOnly && field.FieldType == typeof(string))
        {
            var permission = (string)field.GetValue(null)!;
            authorizationBuilder.AddPolicy(permission, policy => policy.RequireClaim("permission", permission));
        }
    }

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        // app.MapOpenApi();
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseMiddleware<PharmacyStock.API.Middleware.GlobalExceptionMiddleware>();
    app.UseMiddleware<PharmacyStock.API.Middleware.PerformanceMiddleware>();

    app.UseHttpsRedirection();

    app.UseCors("AllowAngular");

    // Add Serilog request logging (logs HTTP requests)
    app.UseSerilogRequestLogging();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapHub<PharmacyStock.API.Hubs.DashboardHub>("/hubs/dashboard");

    // Healthcheck endpoint for Replit
    app.MapGet("/", () => Results.Ok(new { status = "healthy", message = "Pharmacy Stock API is running" }));


    // Seed Data
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<PharmacyStock.Infrastructure.Persistence.Context.AppDbContext>();
        context.Database.Migrate(); // Automatically apply migrations
        PharmacyStock.Infrastructure.Persistence.DataSeeder.SeedUsers(context);
        PharmacyStock.Infrastructure.Persistence.DataSeeder.SeedPermissions(context);
    }

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}