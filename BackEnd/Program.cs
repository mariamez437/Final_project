using Lost_and_Found.Interfaces;
using Lost_and_Found.Models;
using Lost_and_Found.Models.Entites;
using Lost_and_Found.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddCors();
builder.Services.AddDbContext<DataConnection>();
builder.Services.AddScoped<ILostCardService, LostCardService>();
builder.Services.AddScoped<ILostPhoneService, LostPhoneService>();
builder.Services.AddScoped<IFindCardService, FindCardService>();
builder.Services.AddScoped<IFindPhoneService,FindPhoneService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IChecking_For_Items, Checking_For_Items>();
builder.Services.AddAutoMapper(typeof(Program));


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(op =>
    {
        op.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["AppSettings:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["AppSettings:Audience"],
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["AppSettings:Token"]!)),
            ValidateIssuerSigningKey = true
        };
    });



builder.Services.AddSwaggerGen(op =>
{
    op.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Lost-Find (Graduation Project)",
        Description = "Web Api.NetCore"
    });

    // Define the Bearer security scheme
    op.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token in the format: Bearer {your token}"
    });

    // Add the security requirement
    op.AddSecurityRequirement(new OpenApiSecurityRequirement
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

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var con = scope.ServiceProvider.GetRequiredService<DataConnection>();
    var ser = scope.ServiceProvider.GetRequiredService<IAuthService>();

    if (!con.Managers.Any())
        ser.AddManager(new Manager
        {
            EmailManager = "Manager1@gmail",
            Password = "123456789",
            CardID = "303040",
            PhoneNumber = "010207143"
        });
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(op => op.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());
app.UseAuthorization();

app.MapControllers();

app.Run();
