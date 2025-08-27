using Andux.Admin.Domain;
using Andux.Core.EfTrack;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// 获取程序集名
var assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 使用 AddEfOrmFramework 必须加
builder.Services.AddHttpContextAccessor();

// 注册 EF 仓储、工作单元、审计拦截器、DbContext（MySQL）
builder.Services.AddEfOrmFramework<AnduxAdminDbContext>(builder.Configuration, new Version(8, 0, 32));


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
