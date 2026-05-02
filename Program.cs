<<<<<<< HEAD
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Azure;
using Azure.AI.OpenAI;
using SmartLMS.Application.Interfaces;
using SmartLMS.Application.Services;
using SmartLMS.Infrastructure.Persistance;

=======
using Microsoft.EntityFrameworkCore;
using SmartLMS.Infrastructure.Persistance;
>>>>>>> origin/main
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

<<<<<<< HEAD
// Entra ID Authentication with multi-role authorization
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireStudentRole", policy => policy.RequireRole("Student"));
    options.AddPolicy("RequireInstructorRole", policy => policy.RequireRole("Instructor"));
    options.AddPolicy("RequireAdministratorRole", policy => policy.RequireRole("Administrator"));
});

builder.Services.AddControllers();

=======
builder.Services.AddControllers();
>>>>>>> origin/main
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

<<<<<<< HEAD
// Database - EF Core for writes
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Dapper Repository for high-performance reads
builder.Services.AddScoped<ICourseRepository, CourseRepository>();

// AI Services
builder.Services.AddSingleton(sp =>
{
    var endpoint = builder.Configuration["AzureOpenAI:Endpoint"] 
        ?? throw new InvalidOperationException("Azure OpenAI Endpoint not configured");
    var apiKey = builder.Configuration["AzureOpenAI:ApiKey"] 
        ?? throw new InvalidOperationException("Azure OpenAI API Key not configured");
    
    return new OpenAIClient(new Uri(endpoint), new AzureKeyCredential(apiKey));
});

builder.Services.AddScoped<IChatService, ChatService>();

// CORS (configure as needed for your frontend)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

=======
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

>>>>>>> origin/main
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

<<<<<<< HEAD
app.UseCors();

app.UseAuthentication();
=======
>>>>>>> origin/main
app.UseAuthorization();

app.MapControllers();

app.Run();
<<<<<<< HEAD

=======
>>>>>>> origin/main
