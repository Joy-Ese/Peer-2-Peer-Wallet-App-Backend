global using Microsoft.EntityFrameworkCore;
global using WalletPayment.Services.Data;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Identity;
using WalletPayment.Models.DataObjects;
using WalletPayment.Services.Interfaces;
using WalletPayment.Services.Services;
using WalletPayment.Validation.Validators;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        builder =>
        {
            //builder.WithOrigins("");
            builder.AllowAnyOrigin();
            builder.AllowAnyMethod();
            builder.AllowAnyHeader();
        });
});

builder.Services.AddControllers();

builder.Services.AddFluentValidation(x =>
{
    x.ImplicitlyValidateChildProperties = true;
});

builder.Services.AddDbContext<DataContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Services are injected here to be available app wide
builder.Services.AddScoped<IUser, UserService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<IValidator<UserSignUpDto>, UserValidator>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
