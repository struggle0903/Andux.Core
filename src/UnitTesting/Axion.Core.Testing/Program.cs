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
    // ��� Bearer Token �����֤�� Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "���������� Token",
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

// ʹ�� AddEfOrmFramework �����
builder.Services.AddHttpContextAccessor();

// ע�� EF �ִ���������Ԫ�������������DbContext��MySQL��
builder.Services.AddEfOrmFramework<AppDbContext>(builder.Configuration, new Version(8, 0, 32));

#endregion

#region Andux.Core.Logger

// ������־���ò�ע�� Serilog
builder.Services.AddSerilogLogging(builder.Configuration);

#endregion

#region Andux.Core.Redis
builder.Services.AddRedisService(builder.Configuration);
#endregion

#region Andux.Core.RabbitMQ

//// ���RabbitMQ��ط���(���⻧ע��)
//builder.Services.UseAnduxRabbitMQServices(builder.Configuration, null, [
//    new("root") { Password = "mq@20241029!." },
//    new("bsb") { Password = "bsb@hyhf!.." },
//    new("sfm") { Password = "sfm@hyhf!.." }
//]);

////// ���RabbitMQ��ط���(�⻧ģʽ)
////builder.Services.UseAnduxRabbitMQServices(builder.Configuration, "bsb", [
////    new("root") { Password = "mq@20241029!." },
////    new("bsb") { Password = "bsb@hyhf!.." },
////    new("sfm") { Password = "sfm@hyhf!.." }
////]);

//// 5. ע���̨����
//builder.Services.AddHostedService<OrderProcessingService>();

//// ���������������
////builder.Services.AddHostedService<OrderProcessingService>();

#endregion

#region Andux.Core.Helper

builder.Services.UseAnduxHelper();

#endregion

#region Andux.Core.SignalR

builder.Services.UseAnduxSignalR(new SignalROptions
{
    // �ֲ�ʽ��Ⱥ������Ҫ
    RedisConnection = "localhost:6379,defaultDatabase=1,password=Aa123456"
    //RedisConnection = null
});

builder.Services.AddHostedService<SignalRClient1Service>();
builder.Services.AddHostedService<SignalRClient2Service>();
builder.Services.AddHostedService<SignalRClient3Service>();

#endregion

var app = builder.Build();
app.UseRouting();

#region ��̬redis�÷�
// ������������ע���õ��� IRedisService ��ʵ��
var redisService = app.Services.GetRequiredService<IRedisService>();

// ע�뾲̬ RedisHelper
RedisHelper.Configure(redisService);
#endregion

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.MapHub<AnduxChatHub>("/chatHub"); //�ڴ��

app.MapHub<AnduxRedisChatHub>("/chatHub"); //redis��

app.UseHttpsRedirection();

// ������֤����Ȩ�м����˳���ܴ�
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
