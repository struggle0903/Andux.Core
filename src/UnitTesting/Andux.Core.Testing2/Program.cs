using Andux.Core.Common.EventBusDto;
using Andux.Core.EventBus.Events;
using Andux.Core.EventBus.Extensions;
using Andux.Core.Testing2.Events;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

#region Andux.Core.EventBus
builder.Services.UseAnduxEventBus(builder.Configuration);

// ��־������ע��
builder.Services.AddSingleton<AddOrderEventHandler>();
builder.Services.AddSingleton<IEventHandler<AddOrderEvent>, AddOrderEventHandler>();

// ͬ�����ݴ�����ע��
builder.Services.AddSingleton<SyncDataEventHandler>();
builder.Services.AddSingleton<IEventHandler<SyncDataEvent>, SyncDataEventHandler>();
#endregion

var app = builder.Build();


#region Andux.Core.EventBus
// ��ʼ���¼����ģ��Ƽ�������ʱ��
app.Lifetime.ApplicationStarted.Register(async () =>
{
    var bus = app.Services.GetRequiredService<IEventBus>();
    await bus.SubscribeAsync<AddOrderEvent, AddOrderEventHandler>();
    await bus.SubscribeAsync<SyncDataEvent, SyncDataEventHandler>();
});
#endregion

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
