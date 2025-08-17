using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using TaskFlow.API.Middleware;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Common.Behaviors;
using TaskFlow.Application.Common.Mapping;
using TaskFlow.Application.Configuration;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;
using TaskFlow.Infrastructure.Auth;
using TaskFlow.Infrastructure.Data;
using TaskFlow.Infrastructure.Repositories;
using TaskFlow.Infrastructure.Services;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("Logs/log.txt", rollingInterval: RollingInterval.Day)
    .Enrich.FromLogContext()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

builder.Services.AddDbContext<TaskFlowDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default"))
);

var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtOptions = jwtSection.Get<JwtOptions>();
builder.Services.Configure<JwtOptions>(jwtSection);
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddAuthentication(options =>
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
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions?.Issuer ?? throw new InvalidOperationException("JWT Issuer is not configured."),
            ValidAudience = jwtOptions.Audience ?? throw new InvalidOperationException("JWT Audience is not configured."),
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret ?? throw new InvalidOperationException("JWT Secret is not configured."))),
        };
    });

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(TaskFlow.Application.AssemblyReference).Assembly);
    cfg.AddOpenBehavior(typeof(UserBoardAuthorizationBehavior<,>));
});

builder.Services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>(), typeof(TaskFlow.Application.AssemblyReference).Assembly);

builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection("EmailOptions"));
builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter: ''Bearer -token-''",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();