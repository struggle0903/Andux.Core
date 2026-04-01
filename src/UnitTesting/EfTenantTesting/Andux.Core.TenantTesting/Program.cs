using Andux.Core.EfTenant;
using Andux.Core.Tenant.Application;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 注册平台库
builder.Services.AddDbContext<BizTenantContext>(options =>
{
    options.UseMySql(
        builder.Configuration.GetConnectionString("Tenant"),
        new MySqlServerVersion(new Version(8, 0, 36)));
});

// 映射接口
builder.Services.AddScoped<ITenantDbContext>(sp => sp.GetRequiredService<BizTenantContext>());

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
