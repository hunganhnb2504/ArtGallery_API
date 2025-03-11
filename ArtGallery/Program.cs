using ArtGallery.Entities;
using ArtGallery.Helper;
using ArtGallery.Service.Artists;
using ArtGallery.Service.Email;
using ArtGallery.Service.IMG;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://*:{port}");
builder.Services.AddHealthChecks();
//builder.Services.AddDbContext<ArtGalleryApiContext>(options =>
//   options.UseSqlServer(builder.Configuration.GetConnectionString("ArtGalleryApiContext") ?? throw new InvalidOperationException("Connection string 'ArtGalleryApiContext' not found.")));
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:3000/", "http://localhost:5000/");
        policy.AllowAnyOrigin();
        policy.AllowAnyMethod();
        policy.AllowAnyHeader();
    });
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddTransient<IEmailService, EmailService>();
builder.Services.AddScoped<IImgService, ImgService>();
builder.Services.AddScoped<IArtistService, ArtistService>();

builder.Services.AddControllers().AddNewtonsoftJson(options =>
                                   options.SerializerSettings.ReferenceLoopHandling
                                   = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                                   );
// Add services to the container.
var Key = builder.Configuration["JWT:Key"];
var KeyBytes = Encoding.UTF8.GetBytes(Key);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(opt =>
                {
                    opt.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidAudience = builder.Configuration["JWT:Audience"],
                        ValidIssuer = builder.Configuration["JWT:Issuer"],
                        IssuerSigningKey = new SymmetricSecurityKey(KeyBytes),
                    };
                });

builder.Services.AddControllers();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "MyWebApiApp", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
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

try
{
    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }
    app.UseHealthChecks("/health");

    // Configure the HTTP request pipeline.
    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseStaticFiles();
    app.MapControllers();
    var webSocketOptions = new WebSocketOptions
    {
        KeepAliveInterval = TimeSpan.FromMinutes(2),
    };
    app.UseWebSockets(webSocketOptions);
    app.UseCors();

    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine("❌ Application startup failed!");
    Console.WriteLine($"🔍 Error: {ex.Message}");
    Console.WriteLine($"📌 StackTrace: {ex.StackTrace}");
}