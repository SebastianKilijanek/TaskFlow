using System.Security.Claims;
using System.Text;
using System.Text.Json;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskFlow.Application.Common.Behaviors;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Common.Mapping;
using TaskFlow.Application.Configuration;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;
using TaskFlow.Infrastructure.Auth;
using TaskFlow.Infrastructure.Data;
using TaskFlow.Infrastructure.Repositories;

namespace TaskFlow.Tests;

public record TestScope(IServiceScope ServiceScope, TaskFlowDbContext DbContext, IUnitOfWork UnitOfWork, IMapper Mapper, IMediator Mediator);

public class TestWebApplicationFactory<T> : WebApplicationFactory<T> where T : class
{
    private readonly string _connectionString;

    protected virtual bool UseTestAuthentication => true;

    public TestWebApplicationFactory()
    {
        _connectionString = PostgreSqlFixture.CreateDatabase();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services => {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<TaskFlowDbContext>));
            if (descriptor != null) services.Remove(descriptor);

            services.AddDbContext<TaskFlowDbContext>(options => {
                options.UseNpgsql(_connectionString);
            });

            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

            if (UseTestAuthentication)
            {
                services.AddAuthentication("Test")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });
                services.PostConfigure<AuthenticationOptions>(options =>
                {
                    options.DefaultAuthenticateScheme = "Test";
                    options.DefaultChallengeScheme = "Test";
                });
            }
            
            services.AddScoped<IUnitOfWork, UnitOfWork>();    
            
            services.AddMediatR(cfg => {
                cfg.RegisterServicesFromAssembly(typeof(TaskFlow.Application.AssemblyReference).Assembly);
                cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
                cfg.AddOpenBehavior(typeof(UserExistenceCheckBehavior<,>));
                cfg.AddOpenBehavior(typeof(EntityExistenceCheckBehavior<,>));
                cfg.AddOpenBehavior(typeof(BoardAuthorizationBehavior<,>));
            });

            services.AddValidatorsFromAssembly(typeof(TaskFlow.Application.AssemblyReference).Assembly, ServiceLifetime.Scoped, includeInternalTypes: true);
            
            services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>(), typeof(TaskFlow.Application.AssemblyReference).Assembly);

            services.Configure<EmailOptions>(options =>
            {
                options.SmtpHost = "";
                options.SmtpPort = 0;
                options.SmtpUser = "";
                options.SmtpPass = "";
                options.From = "";
                options.EnableSsl = true;
            });
            services.AddScoped<IEmailService, TestEmailService>();
            
            services.AddControllers();
        });
    }
    
    public TestScope GetTestScope()
    {
        var serviceScope = Services.CreateScope();
        
        var context = serviceScope.ServiceProvider.GetRequiredService<TaskFlowDbContext>();
        var unitOfWork = serviceScope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var mapper = serviceScope.ServiceProvider.GetRequiredService<IMapper>();
        var mediator = serviceScope.ServiceProvider.GetRequiredService<IMediator>();
        
        PostgreSqlFixture.DropDatabase(_connectionString);
        context.Database.EnsureCreated();
        
        return new TestScope(serviceScope, context, unitOfWork, mapper, mediator);
    }
    
    public HttpClient CreateClientWithClaims(params Claim[] claims)
        => CreateClientForUser(Guid.Parse(TestAuthHandler.UserId), claims);

    public HttpClient CreateClientForUser(Guid userId, params Claim[] extraClaims)
    {
        var client = CreateClient();
        var claimsList = new List<Claim>
        {
            new(ClaimTypes.Name, "Test"),
            new(ClaimTypes.NameIdentifier, userId.ToString()),
        };

        if (extraClaims.Length > 0)
        {
            claimsList.AddRange(extraClaims);
        }

        var claimsJson = JsonSerializer.Serialize(claimsList.Select(c => new { c.Type, c.Value }));
        var base64Claims = Convert.ToBase64String(Encoding.UTF8.GetBytes(claimsJson));
        client.DefaultRequestHeaders.Add(TestAuthHandler.TestClaimsHeader, base64Claims);

        return client;
    }
}

public class JwtWebApplicationFactory<T> : TestWebApplicationFactory<T> where T : class
{
    protected override bool UseTestAuthentication => false;
}