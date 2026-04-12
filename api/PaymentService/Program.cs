using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PaymentService.Data;
using PaymentService.Services;
using PaymentService.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddControllers();

// Database
builder.Services.AddDbContext<PaymentContext>(options =>
    options.UseSqlServer(builder.Configuration["ConnectionStrings:PaymentDb"]));

// JWT Authentication (shared key with UserManagementService)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddAuthorization();

// HttpClient for inter-service communication
builder.Services.AddHttpClient("TrainTracking", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:TrainTrackingServiceUrl"] ?? "http://localhost:5176");
});
builder.Services.AddHttpClient("TicketQR", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:TicketQRServiceUrl"] ?? "http://localhost:5185");
});

// Services
builder.Services.AddScoped<ITicketService, TicketService>();
builder.Services.AddScoped<IPaymentProcessingService, PaymentProcessingService>();

var app = builder.Build();

app.UseCors(policy => policy
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()
    .WithOrigins("http://localhost:4200"));

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<PaymentContext>();
    await context.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
