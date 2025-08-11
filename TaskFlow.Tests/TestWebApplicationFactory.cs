using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskFlow.Application.Common.Mapping;
using TaskFlow.Application.Configuration;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;
using TaskFlow.Infrastructure.Auth;
using TaskFlow.Infrastructure.Data;
using TaskFlow.Infrastructure.Repositories;

namespace TaskFlow.Tests;

public record TestScope(IServiceScope ServiceScope, TaskFlowDbContext DbContext, IUnitOfWork UnitOfWork, IMapper Mapper);

public class TestWebApplicationFactory<T> : WebApplicationFactory<T> where T : class
{
    private readonly string _dbName = $"TestDb_{Guid.NewGuid()}";
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services => {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<TaskFlowDbContext>));
            if (descriptor != null) services.Remove(descriptor);

            services.AddDbContext<TaskFlowDbContext>(options => {
                options.UseInMemoryDatabase(_dbName);
            });
            
            // Configure JWT options properly
            services.Configure<JwtOptions>(options =>
            {
                options.Issuer = "TestIssuer";
                options.Audience = "TestAudience";
                options.Secret = "7ee17f31088c745fcf48d8b60d2e26db5a8cca684e19b86e98d8a24081159f63aa4b1b6caea74e26ad6f050e2e6895a76e92fe5315cb4156a77593520a59c501";
                options.ExpiryMinutes = 60;
                options.RefreshExpiryMinutes = 43200;
            });
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
            services.AddAuthentication("Test")
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });
            services.PostConfigure<AuthenticationOptions>(options =>
            {
                options.DefaultAuthenticateScheme = "Test";
                options.DefaultChallengeScheme = "Test";
            });
            
            services.AddScoped<IUnitOfWork, UnitOfWork>();    
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(TaskFlow.Application.AssemblyReference).Assembly));
            services.AddAutoMapper(typeof(MappingProfile));
            services.AddControllers();
        });
    }
    
    public TestScope GetTestScope()
    {
        var serviceScope = Services.CreateScope();
        
        var context = serviceScope.ServiceProvider.GetRequiredService<TaskFlowDbContext>();
        var unitOfWork = serviceScope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var mapper = serviceScope.ServiceProvider.GetRequiredService<IMapper>();
        
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
        
        return new TestScope(serviceScope, context, unitOfWork, mapper);
    }
}