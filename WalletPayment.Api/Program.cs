global using Microsoft.EntityFrameworkCore;
global using WalletPayment.Services.Data;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using System.Text;
using WalletPayment.Models.DataObjects;
using WalletPayment.Services.Interfaces;
using WalletPayment.Services.Services;
using WalletPayment.Validation.Validators;
using NLog;
using NLog.Web;
using Microsoft.AspNetCore.Cors.Infrastructure;
using DinkToPdf.Contracts;
using DinkToPdf;
using WalletPayment.Api;

var logger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
logger.Debug("init main");

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.

    builder.Services.AddCors((options) =>
    {
        options.AddPolicy("NotificationClientApp",
            new CorsPolicyBuilder()
            .WithOrigins("http://localhost:4200")
            .WithOrigins("http://localhost:51471")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .SetIsOriginAllowed(origin => true)
            .AllowCredentials()
            .Build());
    });


    // NLog: Setup NLog for Dependency injection
    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    builder.Services.AddControllers();

    builder.Services.AddFluentValidationAutoValidation().AddFluentValidationClientsideAdapters();

    // For EntityFramework
    builder.Services.AddDbContext<DataContext>(options =>
    {
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    });

    // Services are injected here to be available app wide
    builder.Services.AddScoped<IAuth, AuthService>();
    builder.Services.AddScoped<IDashboard, DashboardService>();
    builder.Services.AddScoped<IAccount, AccountService>();
    builder.Services.AddScoped<ITransaction, TransactionService>();
    builder.Services.AddScoped<IEmail, EmailService>();
    builder.Services.AddScoped<IPayment, PaymentService>();
    builder.Services.AddScoped<INotification, NotificationService>();
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddTransient<IValidator<UserSignUpDto>, UserValidator>();
    builder.Services.AddSignalR();
    builder.Services.AddLogging();
    builder.Services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));

    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
        {
            Description = "Standard Authorization Header using the Bearer Scheme (\"bearer {token}\")",
            In = ParameterLocation.Header,
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey
        });

        options.OperationFilter<SecurityRequirementsOperationFilter>();
    });

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options => {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8
                    .GetBytes(builder.Configuration.GetSection("AppSettings:Token").Value)),
                ValidateIssuer = false,
                ValidateAudience = false
            };
        });


    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    // Migrate latest database changes during startup
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<DataContext>();
        dbContext.Database.Migrate();
    }

    app.UseStaticFiles();

    app.UseCors("NotificationClientApp");

    app.UseHttpsRedirection();

    app.UseAuthentication();

    app.UseRouting();

    app.UseAuthorization();

    app.MapHub<NotificationSignalR>("/notification");

    app.MapControllers();

    app.Seed();

    app.Run();
}
catch (Exception exception)
{
    // NLog: catch setup errors
    logger.Error(exception, "Stopped program because of exception");
    throw;
}
finally
{
    // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
    NLog.LogManager.Shutdown();
}