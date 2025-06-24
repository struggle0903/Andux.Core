using Andux.Core.EfTrack;
using Andux.Core.Helper.Extensions;
using Andux.Core.Logger;
using Andux.Core.RabbitMQ.Extensions;
using Andux.Core.Redis.Extensions;
using Andux.Core.Redis.Helper;
using Andux.Core.Redis.Services;
using Andux.Core.SignalR;
using Andux.Core.SignalR.Clients;
using Andux.Core.SignalR.Extensions;
using Andux.Core.SignalR.Hubs;
using Andux.Core.Testing;
using Andux.Core.Testing.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.OpenApi.Models;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://127.0.0.1:5001");

builder.Services.AddControllers(opt =>
{
    opt.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
}).AddJsonOptions(options =>
{
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull | JsonIgnoreCondition.WhenWritingDefault;
    options.JsonSerializerOptions.AllowTrailingCommas = false;
    options.JsonSerializerOptions.WriteIndented = true;
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = "your-app",
        ValidAudience = "your-client",
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes("YourSuperSecretKeyForJwtToken123!@#")
        ),

        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // 添加 Bearer Token 身份验证到 Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "请输入您的 Token",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
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
            Array.Empty<string>()
        }
    });
});

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

//// 添加RabbitMQ相关服务(非租户注册)
//builder.Services.UseAnduxRabbitMQServices(builder.Configuration, null, [
//    new("root") { Password = "mq@20241029!." },
//    new("bsb") { Password = "bsb@hyhf!.." },
//    new("sfm") { Password = "sfm@hyhf!.." }
//]);

////// 添加RabbitMQ相关服务(租户模式)
////builder.Services.UseAnduxRabbitMQServices(builder.Configuration, "bsb", [
////    new("root") { Password = "mq@20241029!." },
////    new("bsb") { Password = "bsb@hyhf!.." },
////    new("sfm") { Password = "sfm@hyhf!.." }
////]);

//// 5. 注册后台服务
//builder.Services.AddHostedService<OrderProcessingService>();

//// 监听订单处理服务
////builder.Services.AddHostedService<OrderProcessingService>();

#endregion

#region Andux.Core.Helper

builder.Services.UseAnduxHelper();

#endregion

#region Andux.Core.SignalR

builder.Services.UseAnduxSignalR(new SignalROptions
{
    // 分布式集群部署需要
    RedisConnection = "localhost:6379,defaultDatabase=1,password=Aa123456"
    //RedisConnection = null
});

builder.Services.AddHostedService<SignalRClient1Service>();
builder.Services.AddHostedService<SignalRClient2Service>();
builder.Services.AddHostedService<SignalRClient3Service>();

#endregion

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

//app.MapHub<AnduxChatHub>("/chatHub"); //内存版

app.MapHub<AnduxRedisChatHub>("/chatHub"); //redis版

app.UseHttpsRedirection();

// 启用认证和授权中间件（顺序不能错）
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
