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
using Andux.Core.Testing;
using Andux.Core.Testing.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://0.0.0.0:5001");  // ȷ��������������ӿ�

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

#region Andux.Core.Helper

builder.Services.UseAnduxHelper();

#endregion

#region Andux.Core.SignalR

builder.Services.UseAnduxSignalR(new SignalROptions
{
    // �ֲ�ʽ��Ⱥ������Ҫ
    //RedisConnection = "localhost:6379"
    RedisConnection = null
});

var client = new SignalRClient("http://127.0.0.1:5001/chatHub");

client.On<string, string>("ReceiveMessage", (user, msg) =>
{
    Console.WriteLine($"{user}: {msg}");
});


await client.ConnectAsync();

await client.SendMessageAsync("SendMessage", "�ͻ����û�", "��� SignalR");

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

app.MapHub<ChatHub>("/chatHub");

app.UseHttpsRedirection();

// ������֤����Ȩ�м����˳���ܴ�
app.UseAuthentication(); // �������� UseAuthorization
app.UseAuthorization();

app.MapControllers();

app.Run();
