using AutoMapper;
using Dao;
using Dao.Impl;
using Dao.Interface;
using Entities;
using ExternalApi.Impls;
using ExternalApi.Interfaces;
using Microsoft.EntityFrameworkCore;
using Services.Impl;
using Services.Interfaces;
using WebApi.Controllers;
using WebApi.Controllers.Meetings;
using WebApi.Controllers.Sessions;
using WebApi.ViewModels;
using WebApi.ViewModels.Meetings;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ApiF1DB>(option =>
{
    option.UseSqlServer(builder.Configuration.GetConnectionString("F1DBLocal"));
});

builder.Services.AddAutoMapper(cfg =>
{
    cfg.CreateMap<Meeting, MeetingViewModel>();
    cfg.CreateMap<MeetingViewModel, Meeting>();

    cfg.CreateMap<Session, SessionViewModel>();
    cfg.CreateMap<SessionViewModel, Session>();
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
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
