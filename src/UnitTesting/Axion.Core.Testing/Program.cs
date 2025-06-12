using Andux.Core.EfTrack;
using Andux.Core.Logger;
using Andux.Core.Redis.Extensions;
using Andux.Core.Redis.Helper;
using Andux.Core.Redis.Services;
using Andux.Core.Testing;
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
builder.Services.AddControllers(); 

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
