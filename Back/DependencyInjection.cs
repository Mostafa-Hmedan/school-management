using System.Text;
using System.Threading.RateLimiting;
using Back.Data;
using Back.Entities;
using Back.Interfaces;
using Back.OpenApi.Transformers;
using Back.Services;
using Back.Services.Image;
using Back.Services.Interfaces;
using Back.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.CodeAnalysis.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Back;

public static class DependencyInjection
{
    public static IServiceCollection AddDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddDatabase(configuration)
            .AddIdentityConfig()
            .AddJwtAuthentication(configuration)
            .AddApplicationServices()
            .AddValidations()
            .AddVersioning()
            .AddExceptionHandling()
            .AddApiDocumentation()
            .AddCaching()
            .AddRateLimiting()
            .AddCorsPolicy();
        return services;
    }

    // ───────────── Database ─────────────
    // ───────────── Database ─────────────
    private static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

        return services;
    }




    // ───────────── Identity ─────────────
    // ───────────── Identity ─────────────
    private static IServiceCollection AddIdentityConfig(this IServiceCollection services)
    {
        services.AddIdentity<AppUser, IdentityRole>(options =>
       {
           options.Lockout.MaxFailedAccessAttempts = 5;  // بعد 5 محاولات فاشلة
           options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5); // يُقفل 5 دقيقة
           options.Lockout.AllowedForNewUsers = true;
       })
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

        return services;
    }

    // ───────────── JWT ─────────────
    // ───────────── JWT ─────────────
    private static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("JwtSettings");
        var key = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!);
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(key)
            };
        });
        services.AddAuthorization();
        return services;
    }



    // ───────────── Application Services ─────────────
    // ───────────── Application Services ─────────────
    private static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<JwtTokenProvider>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IImageService, ImageService>();
        services.AddScoped<IStudentServices, StudentServices>();
        services.AddScoped<ITeacherServices, TeacherServices>();
        services.AddScoped<ISubjectServices, SubjectServices>();
        services.AddScoped<IClassServices, ClassServices>();
        services.AddScoped<IAttendanceServices, AttendanceServices>();
        services.AddScoped<IGradeServices, GradeServices>();
        services.AddScoped<IEnrollmentServices, EnrollmentServices>();
        services.AddScoped<IStudentPaymentServices, StudentPaymentServices>();
        services.AddScoped<ITeacherPaymentServices, TeacherPaymentServices>();
        services.AddScoped<IEmployeeServices, EmployeeServices>();
        services.AddScoped<IEmployeePaymentServices, EmployeePaymentServices>();
        services.AddScoped<ITeacherAvailabilityServices, TeacherAvailabilityServices>();
        services.AddScoped<IClassScheduleServices, ClassScheduleServices>();
        return services;
    }


    // ───────────── Validations ─────────────
    // ───────────── Validations ─────────────
    private static IServiceCollection AddValidations(this IServiceCollection services)
    {
        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssemblyContaining<CreateStudentValidator>();
        return services;
    }



    // ───────────── API Versioning ─────────────
    // ───────────── API Versioning ─────────────
    private static IServiceCollection AddVersioning(this IServiceCollection services)
    {
        services.AddApiVersioning(option =>
        {
            option.DefaultApiVersion = new ApiVersion(1, 0);
            option.AssumeDefaultVersionWhenUnspecified = true;
            option.ReportApiVersions = true;
            option.ApiVersionReader = new UrlSegmentApiVersionReader();
        });
        return services;
    }


    // ───────────── Exception Handling ─────────────
    // ───────────── Exception Handling ─────────────
    private static IServiceCollection AddExceptionHandling(this IServiceCollection services)
    {
        services.AddProblemDetails(option =>
        {
            option.CustomizeProblemDetails = (context) =>
            {
                context.ProblemDetails.Instance =
                    $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";

                context.ProblemDetails.Extensions
                    .Add("requestId", context.HttpContext.TraceIdentifier);
            };
        });
        services.AddExceptionHandler<GlobalExceptionHandler>();
        return services;
    }



    public static IServiceCollection AddApiDocumentation(this IServiceCollection services)
    {
        string[] versions = ["v1", "v2"];

        foreach (var version in versions)
        {
            services.AddOpenApi(version, options =>
            {
                // Versioning config
                options.AddDocumentTransformer<VersionInfoTransformer>();

                // Security Scheme config

                options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
                options.AddOperationTransformer<BearerSecuritySchemeTransformer>();
            });
        }
        return services;
    }



    // ───────────── Caching ─────────────
    private static IServiceCollection AddCaching(this IServiceCollection services)
    {
        services.AddMemoryCache();
        return services;
    }

    // ───────────── Rate Limiting ─────────────
    private static IServiceCollection AddRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // عام — كل الـ Controllers
            options.AddFixedWindowLimiter("fixed", limiterOptions =>
            {
                limiterOptions.PermitLimit = 100;
                limiterOptions.Window = TimeSpan.FromMinutes(1);
                limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                limiterOptions.QueueLimit = 10;
            });

            // Auth — أشد لمنع Brute Force (لكل IP)
            options.AddPolicy("auth", httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromMinutes(1),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    }));
        });

        return services;
    }

    // ───────────── CORS ─────────────
    private static IServiceCollection AddCorsPolicy(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("NextJs", policy =>
            {
                policy.WithOrigins("http://localhost:3000")
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
        });

        return services;
    }
}
