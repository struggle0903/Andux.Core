using Andux.Core.EfTrack;
using Andux.Core.Helper.Extensions;
using Andux.Core.Logger;
using Andux.Core.Redis.Extensions;
using Andux.Core.Redis.Helper;
using Andux.Core.Redis.Services;
using Andux.Core.SignalR;
using Andux.Core.SignalR.Extensions;
using Andux.Core.SignalR.Hubs;
using Andux.Core.Testing;
using Andux.Core.Testing.Services;
using Microsoft.OpenApi.Models;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using Andux.Core.RabbitMQ.Extensions;
using Andux.Core.EventBus.Events;
using Andux.Core.EventBus.Extensions;
using Andux.Core.RabbitMQ.Models;
using Andux.Core.Testing.Events;

var builder = WebApplication.CreateBuilder(args);

//builder.WebHost.UseUrls("http://127.0.0.1:5001");

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

// 注册AnduxRabbitMQ，普通模式
//builder.Services.UseAnduxRabbitMQServices(new RabbitMQOptions()
//{
//    Host = builder.Configuration.GetValue("AnduxRabbitMQ:Host", "localhost"),
//    Port = builder.Configuration.GetValue("AnduxRabbitMQ:Port", 5672),
//    UserName = builder.Configuration.GetValue("AnduxRabbitMQ:Username", "guest"),
//    Password = builder.Configuration.GetValue("AnduxRabbitMQ:Password", "guest"),
//    VirtualHost = builder.Configuration.GetValue("AnduxRabbitMQ:VirtualHost", "/"),
//    NetworkRecoveryInterval = builder.Configuration.GetValue("AnduxRabbitMQ:NetworkRecoveryInterval", 10)
//});

// 注册AnduxRabbitMQ，租户模式
var tenantOptions = new List<RabbitMQTenantOptions>
{
    new ()
    {
        TenantId = "andy",
        EnableExchangePrefix = false,
        EnableRoutingKeyPrefix = true,
        EnableQueueNamePrefix = true,
        Host = builder.Configuration.GetValue("AnduxRabbitMQ:Host", "localhost"),
        Port = builder.Configuration.GetValue("AnduxRabbitMQ:Port", 5672),
        UserName = builder.Configuration.GetValue("AnduxRabbitMQ:Username", "guest"),
        Password = builder.Configuration.GetValue("AnduxRabbitMQ:Password", "guest"),
        VirtualHost = builder.Configuration.GetValue("AnduxRabbitMQ:VirtualHost", "/"),
        NetworkRecoveryInterval = builder.Configuration.GetValue("AnduxRabbitMQ:NetworkRecoveryInterval", 10)
    },
    new ()
    {
        TenantId = "pro",
        EnableExchangePrefix = true,
        EnableRoutingKeyPrefix = true,
        EnableQueueNamePrefix = true,
        Host = "111.22.145.28",
        Port = 7093,
        UserName = "test",
        Password = "test@123",
        VirtualHost ="test",
        NetworkRecoveryInterval = builder.Configuration.GetValue("AnduxRabbitMQ:NetworkRecoveryInterval", 10)
    },
    new ()
    {
        TenantId = "hu",
        EnableExchangePrefix = true,
        EnableRoutingKeyPrefix = true,
        EnableQueueNamePrefix = true,
        Host = "111.22.145.236",
        Port = 25704,
        UserName = "log_test",
        Password = "log_test",
        VirtualHost ="/log",
        NetworkRecoveryInterval = builder.Configuration.GetValue("AnduxRabbitMQ:NetworkRecoveryInterval", 10)
    },
};
builder.Services.UseAnduxTenantRabbitMQServices(tenantOptions);

// 监听订单处理服务
//builder.Services.AddHostedService<OrderProcessingService>();

#endregion

#region Andux.Core.Helper

builder.Services.UseAnduxHelper();

#endregion

#region Andux.Core.SignalR

builder.Services.UseAnduxSignalR(new SignalROptions
{
    // 分布式集群部署需要
    // RedisConnection = "localhost:6379,defaultDatabase=1,password=Aa123456"
    RedisConnection = null
});

builder.Services.AddHostedService<SignalRClient1Service>();
builder.Services.AddHostedService<SignalRClient2Service>();
builder.Services.AddHostedService<SignalRClient3Service>();

#endregion

#region Andux.Core.EventBus

builder.Services.UseAnduxEventBus(builder.Configuration);

// 用户创建事件处理器注册
builder.Services.AddSingleton<UserCreatedEventHandler>();
builder.Services.AddSingleton<IEventHandler<UserCreatedEvent>, UserCreatedEventHandler>();

// 日志新增事件处理器注册
builder.Services.AddSingleton<AddLoggerEventHandler>();
builder.Services.AddSingleton<IEventHandler<AddLoggerEvent>, AddLoggerEventHandler>();

#endregion

var app = builder.Build();
app.UseRouting();

#region 静态redis用法
// 假设你用依赖注入拿到了 IRedisService 的实现
var redisService = app.Services.GetRequiredService<IRedisService>();

// 注入静态 RedisHelper
RedisHelper.Configure(redisService);
#endregion

#region Andux.Core.EventBus
// 初始化事件订阅（推荐在启动时）
using (var scope = app.Services.CreateScope())
{
    var eventBus = scope.ServiceProvider.GetRequiredService<IEventBus>();
    await eventBus.SubscribeAsync<UserCreatedEvent, UserCreatedEventHandler>();
    await eventBus.SubscribeAsync<AddLoggerEvent, AddLoggerEventHandler>();
}
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
