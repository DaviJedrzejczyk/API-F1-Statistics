using Dao;
using Dao.Impl;
using Dao.Interface;
using Entities.Class;
using Entities.Dtos;
using ExternalApi.Impls;
using ExternalApi.Interfaces;
using Microsoft.EntityFrameworkCore;
using Services.Impl;
using Services.Interfaces;
using Shared.Converters;
using System.Reflection;
using WebApi.Controllers.Meetings;
using WebApi.ViewModels;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddCors();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ApiF1DB>(option =>
{
    option.UseSqlServer(builder.Configuration.GetConnectionString("F1DBLocal"));
});

builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddMaps(Assembly.GetExecutingAssembly());
});

builder.Services.AddTransient<ISessionDao, SessionDao>();
builder.Services.AddTransient<ISessionService, SessionService>();
builder.Services.AddTransient<IMeetingService, MeetingService>();
builder.Services.AddTransient<IMeetingDao, MeetingDao>();
builder.Services.AddTransient<IUnityOfWork, UnityOfWork>();
builder.Services.AddTransient<ISessionClient, SessionClient>();
builder.Services.AddTransient<IF1ApiClient, F1ApiClient>(); 
builder.Services.AddTransient<IDriverDao, DriverDao>();
builder.Services.AddTransient<IMeetingClient, MeetingClient>();
builder.Services.AddTransient<IDriverService, DriverService>();
builder.Services.AddTransient<IDriverClient, DriverClient>();
builder.Services.AddTransient<ICarDataClient, CarDataClient>();
builder.Services.AddTransient<ICarDataDao, CarDataDao>();
builder.Services.AddTransient<ICarDataService, CarDataService>();
builder.Services.AddTransient<ISessionResultDao, SessionResultDao>();
builder.Services.AddTransient<ISessionResultService, SessionResultService>();
builder.Services.AddTransient<ISessionResultClient, SessionResultClient>();
builder.Services.AddTransient<IOvertakeDao, OvertakeDao>();
builder.Services.AddTransient<IPitDao, PitDao>();
builder.Services.AddTransient<IPitService, PitService>();
builder.Services.AddTransient<IOvertakeService, OvertakeService>();
builder.Services.AddTransient<IPitClient, PitClient>();
builder.Services.AddTransient<IOvertakeClient, OvertakeClient>();
builder.Services.AddTransient<IRaceControlDao, RaceControlDao>();
builder.Services.AddTransient<IRaceControlClient, RaceControlClient>();
builder.Services.AddTransient<IRaceControlService, RaceControlService>();
builder.Services.AddTransient<IStintClient, StintClient>();
builder.Services.AddTransient<IStintDao, StintDao>();
builder.Services.AddTransient<IStintService, StintService>();
builder.Services.AddTransient<ISessionResultQualifyDao, SessionResultQualifyDao>();
builder.Services.AddTransient<ISessionResultQualifyingsService, SessionResultQualifyingsService>();

builder.Services.AddControllers()
    .AddJsonOptions(opts =>
        opts.JsonSerializerOptions.Converters.Add(new ListDoubleNullToZeroConverter()));

builder.Services.AddControllers()
    .AddJsonOptions(opts =>
        opts.JsonSerializerOptions.Converters.Add(new DoubleNullToZeroConverter()));

builder.Services.AddHttpClient<F1ApiClient>();
builder.Services.AddHttpClient<MeetingController>();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Api F1 Statistics");
    });
}

app.UseCors(op =>
{
    op.WithOrigins("https://localhost:7054");
    op.AllowAnyMethod();
    op.AllowAnyHeader();
    op.AllowAnyOrigin();
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
