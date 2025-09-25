using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using EventCalendar.Application.Contracts.Security;
using EventCalendar.Data;
using EventCalendar.Application.Security;
using EventCalendar.Application.Contracts.CivicPlus;
using EventCalendar.Application.CivicPlus;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var corsOrigin = builder.Configuration.GetValue<string>("App:CorsOrigins");
//Add DbContext with SQL Server provider
builder.Services.AddDbContext<UserDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//Add Redis Cache
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});

//Add JWT Authentication middleware
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration.GetValue<string>("AppSettings:Issuer"),
            ValidAudience = builder.Configuration.GetValue<string>("AppSettings:Audience"),
            IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(
                builder.Configuration.GetValue<string>("AppSettings:Token")!
                )),
            ClockSkew = TimeSpan.Zero
        };
    });

//Add Service dependencies
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddHttpClient<IEventsService, EventsService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["CivicPlusService:BaseUrl"]);
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("localhost", builder =>
    {
        //App:CorsOrigins in appsettings.json can contain more than one address with splitted by comma.
        builder
            .WithOrigins(corsOrigin.Split(",").Select(o => o.Trim()).ToArray())
            //.SetIsOriginAllowedToAllowWildcardSubdomains()
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});
builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Event Calendar Service API", Description = "Event Calendar Service API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                },
                Array.Empty<string>()
            }
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Event Calendar Service API");
    c.RoutePrefix = String.Empty;
});

//app.UseAuthentication();
app.UseAuthorization();
app.UseCors("localhost");
app.MapControllers();

app.Run();
