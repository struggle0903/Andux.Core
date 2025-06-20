using Andux.Core.EfTrack;
using Andux.Core.Logger;
using Andux.Core.RabbitMQ.Extensions;
using Andux.Core.RabbitMQ.Interfaces;
using Andux.Core.RabbitMQ.Models;
using Andux.Core.RabbitMQ.Services.Connection;
using Andux.Core.RabbitMQ.Services.Consumers;
using Andux.Core.RabbitMQ.Services.Publishers;
using Andux.Core.RabbitMQ.Services.Tenant;
using Andux.Core.Redis.Extensions;
using Andux.Core.Redis.Helper;
using Andux.Core.Redis.Services;
using Andux.Core.Testing;
using Andux.Core.Testing.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// 注册 Cookie 身份认证
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.LoginPath = "/api/account/login";
        options.LogoutPath = "/api/account/logout";
        options.AccessDeniedPath = "/access-denied";
        options.Cookie.Name = "Andux.Auth";
        options.ExpireTimeSpan = TimeSpan.FromHours(1);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // 解决实体无限循环嵌套报错问题
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

#region Andux.Core.EfTrack

// 使用 AddEfOrmFramework 必须加
builder.Services.AddHttpContextAccessor();

// 注册 EF 仓储、工作单元、审计拦截器、DbContext（MySQL）
builder.Services.AddEfOrmFramework<AppDbContext>(builder.Configuration, new Version(8, 0, 32));

#endregion

#region Andux.Core.Logger

// 加载日志配置并注册 Serilog
builder.Services.AddSerilogLogging(builder.Configuration);

#endregion

#region Andux.Core.Redis
builder.Services.AddRedisService(builder.Configuration);
#endregion

#region Andux.Core.RabbitMQ

// 添加RabbitMQ相关服务(非租户注册)
builder.Services.UseAnduxRabbitMQServices(builder.Configuration, null, [
    new("root") { Password = "mq@20241029!." },
    new("bsb") { Password = "bsb@hyhf!.." },
    new("sfm") { Password = "sfm@hyhf!.." }
]);

//// 添加RabbitMQ相关服务(租户模式)
//builder.Services.UseAnduxRabbitMQServices(builder.Configuration, "bsb", [
//    new("root") { Password = "mq@20241029!." },
//    new("bsb") { Password = "bsb@hyhf!.." },
//    new("sfm") { Password = "sfm@hyhf!.." }
//]);

// 5. 注册后台服务
builder.Services.AddHostedService<OrderProcessingService>();

#endregion

builder.Services.AddHostedService<OrderProcessingService>();

var app = builder.Build();
app.UseRouting();

#region 静态redis用法
// 假设你用依赖注入拿到了 IRedisService 的实现
var redisService = app.Services.GetRequiredService<IRedisService>();

// 注入静态 RedisHelper
RedisHelper.Configure(redisService);
#endregion

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// 启用认证和授权中间件（顺序不能错）
app.UseAuthentication(); // 必须先于 UseAuthorization
app.UseAuthorization();

app.MapControllers();

app.Run();
