using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OnlineBankingApplication.Context;
using OnlineBankingApplication.Middlewares;
using OnlineBankingApplication.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;  
var jwtSettings = configuration.GetSection("Jwt");
var rabbitMQSettings = configuration.GetSection("RabbitMQ");

builder.Services.AddDbContext<BaseDbContext>(options =>
    options.UseInMemoryDatabase("BankingDB"));

builder.Services.AddSingleton(new MessageQueueService(
    rabbitMQSettings["Hostname"], rabbitMQSettings["QueueName"]));

builder.Services.AddSingleton<ResilientHttpClient>();

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]))
        };
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "OnlineBankingApp", Version = "v1" });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ErrorHandlingMiddleware>();

//app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
