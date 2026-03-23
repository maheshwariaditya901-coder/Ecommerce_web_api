using Ecommerce_web_api.Data;
using Ecommerce_web_api.Services;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// 1. Add CORS service

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontendAuth", policy =>
    {
        policy.WithOrigins(
            "http://localhost:5173",
            "https://localhost:7259",
            "http://localhost:5204"
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});


// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// EF Core
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("dbcs")));

// Services
builder.Services.AddScoped<AuthService>();

var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]);

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
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],

        IssuerSigningKey = new SymmetricSecurityKey(key)
    };

    // ADD THIS BLOCK
    // Tell ASP.NET Core to look for the token inside the incoming cookies
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // If the cookie contains the token, extract it! 
            // Note: "jwtToken" MUST exactly match the name you used in Response.Cookies.Append()
            if (context.Request.Cookies.ContainsKey("jwtToken"))
            {
                context.Token = context.Request.Cookies["jwtToken"];
            }

            return Task.CompletedTask;
        }
    };
});

var app = builder.Build();

 

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowFrontendAuth");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();