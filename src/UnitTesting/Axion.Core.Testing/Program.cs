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

// ע�� Cookie �����֤
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
        // ���ʵ������ѭ��Ƕ�ױ�������
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

// ���RabbitMQ��ط���(���⻧ע��)
builder.Services.UseAnduxRabbitMQServices(builder.Configuration, null, [
    new("root") { Password = "mq@20241029!." },
    new("bsb") { Password = "bsb@hyhf!.." },
    new("sfm") { Password = "sfm@hyhf!.." }
]);

//// ���RabbitMQ��ط���(�⻧ģʽ)
//builder.Services.UseAnduxRabbitMQServices(builder.Configuration, "bsb", [
//    new("root") { Password = "mq@20241029!." },
//    new("bsb") { Password = "bsb@hyhf!.." },
//    new("sfm") { Password = "sfm@hyhf!.." }
//]);

// 5. ע���̨����
builder.Services.AddHostedService<OrderProcessingService>();

#endregion

builder.Services.AddHostedService<OrderProcessingService>();

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

app.UseHttpsRedirection();

// ������֤����Ȩ�м����˳���ܴ�
app.UseAuthentication(); // �������� UseAuthorization
app.UseAuthorization();

app.MapControllers();

app.Run();
