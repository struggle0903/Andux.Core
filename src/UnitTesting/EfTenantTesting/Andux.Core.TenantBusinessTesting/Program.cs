using Andux.Core.EfTenant;
using Andux.Core.EfTenant.Extensions;
using Andux.Core.TenantTesting.Application;
using Andux.Core.TenantTesting.Application.Services;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

var assemblyName = Assembly.GetExecutingAssembly().GetName().Name ?? string.Empty;

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new()
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "输入: Bearer {token}"
    });

    c.AddSecurityRequirement(new()
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new()
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddDbContext<TenantDbContext>(options =>
{
    options.UseMySql(
        builder.Configuration.GetConnectionString("Tenant"),
        new MySqlServerVersion(new Version(8, 0, 36)));
});

// 注册业务库（Andux.Core.EfTenant）
builder.Services.AddMultiTenantKit<AdminContext>(builder.Configuration);

builder.Services.AddScoped<IUserService, UserService>();

// 认证服务配置
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = "http://192.168.1.88:16320"; 
        options.TokenValidationParameters = new()
        {
            ValidateAudience = false
        };

        options.RequireHttpsMetadata = false;
    });

builder.Services.AddAuthorization();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
