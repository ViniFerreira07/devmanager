using DevManager.Application.Interfaces;
using DevManager.Application.Mappings;
using DevManager.Application.Services;
using DevManager.Application.Validators;
using DevManager.Domain.Entities;
using DevManager.Domain.Interfaces;
using DevManager.Infrastructure;
using DevManager.Infrastructure.Identity;
using DevManager.Infrastructure.Persistence;
using DevManager.Infrastructure.Repositories;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace DevManager.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDevManagerServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<DevManagerDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IStateService, StateService>();
        services.AddScoped<ICityService, CityService>();
        services.AddScoped<IProgrammingLanguageService, ProgrammingLanguageService>();
        services.AddScoped<IDeveloperService, DeveloperService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IDeveloperRepository, DeveloperRepository>();
        services.AddScoped<IProgrammingLanguageRepository, ProgrammingLanguageRepository>();
        services.AddScoped<ICityRepository, CityRepository>();
        services.AddScoped<IStateRepository, StateRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<DevManagerDbContext>());

        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddAutoMapper(typeof(MappingProfile));
        services.AddValidatorsFromAssemblyContaining<CreateStateRequestValidator>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"] ?? "devmanager-secret-key-2026"))
                };
            });

        services.AddAuthorization();
        services.AddCors(options =>
        {
            options.AddPolicy("Frontend", policy =>
                policy.WithOrigins(configuration["Frontend:Url"] ?? "http://localhost:3000")
                    .AllowAnyHeader()
                    .AllowAnyMethod());
        });
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.EnableAnnotations();
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "DevManager API",
                Version = "v1",
                Description = "Corporate developer management API with JWT, soft delete, reports and CRUD endpoints."
            });
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter your JWT token."
            });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }
}
