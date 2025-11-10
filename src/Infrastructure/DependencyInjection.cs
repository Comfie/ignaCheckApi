using System.Text;
using IgnaCheck.Application.Common.Interfaces;
using IgnaCheck.Domain.Constants;
using IgnaCheck.Infrastructure.AI;
using IgnaCheck.Infrastructure.Data;
using IgnaCheck.Infrastructure.Data.Interceptors;
using IgnaCheck.Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddInfrastructureServices(this IHostApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("IgnaCheckDb");
        Guard.Against.Null(connectionString, message: "Connection string 'IgnaCheckDb' not found.");

        builder.Services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        builder.Services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();
        builder.Services.AddScoped<ISaveChangesInterceptor, TenantSecurityInterceptor>();

        builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
            options.UseNpgsql(connectionString);
            options.ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
        });

        builder.Services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        builder.Services.AddScoped<ApplicationDbContextInitialiser>();

        builder.Services
            .AddDefaultIdentity<ApplicationUser>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        // Configure JWT Authentication
        var jwtSecret = builder.Configuration["Jwt:Secret"];
        if (!string.IsNullOrEmpty(jwtSecret))
        {
            var key = Encoding.UTF8.GetBytes(jwtSecret);

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = true; // Set to false for development if needed
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "IgnaCheck.ai",
                    ValidAudience = builder.Configuration["Jwt:Audience"] ?? "IgnaCheck.ai",
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero
                };

                // Configure events for better error handling
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                        {
                            context.Response.Headers.Append("Token-Expired", "true");
                        }
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = 401;
                        context.Response.ContentType = "application/json";
                        var result = System.Text.Json.JsonSerializer.Serialize(new
                        {
                            error = "Unauthorized",
                            message = "You are not authorized to access this resource."
                        });
                        return context.Response.WriteAsync(result);
                    }
                };
            });
        }

        builder.Services.AddAuthorizationBuilder();

        // AI Configuration
        builder.Services.Configure<AIConfiguration>(builder.Configuration.GetSection(AIConfiguration.SectionName));

        // Application services
        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddTransient<IIdentityService, IdentityService>();
        builder.Services.AddScoped<ITenantService, TenantService>();
        builder.Services.AddTransient<IEmailService, IgnaCheck.Infrastructure.Services.EmailService>();
        builder.Services.AddScoped<IJwtTokenGenerator, IgnaCheck.Infrastructure.Services.JwtTokenGenerator>();
        builder.Services.AddScoped<IFileStorageService, IgnaCheck.Infrastructure.Services.LocalFileStorageService>();
        builder.Services.AddScoped<IDocumentParsingService, IgnaCheck.Infrastructure.Services.DocumentParsingService>();
        builder.Services.AddScoped<IAIAnalysisService, IgnaCheck.Infrastructure.Services.AIAnalysisService>();

        builder.Services.AddAuthorization(options =>
            options.AddPolicy(Policies.CanPurge, policy => policy.RequireRole(Roles.Administrator)));
    }
}
