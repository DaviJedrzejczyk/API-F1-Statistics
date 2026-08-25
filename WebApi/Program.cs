using AutoMapper;
using Dao;
using Dao.Impl;
using Dao.Interface;
using Entities;
using ExternalApi.Impls;
using ExternalApi.Interfaces;
using ExternalApi.ViewModels;
using Microsoft.EntityFrameworkCore;
using Services.Impl;
using Services.Interfaces;
using WebApi.Controllers.Meetings;
using WebApi.ViewModels.DriversViews;
using WebApi.ViewModels.MeetingsViews;
using WebApi.ViewModels.SessionsViews;

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
    //Web API
    cfg.CreateMap<Meeting, MeetingViewModel>();
    cfg.CreateMap<MeetingViewModel, Meeting>();

    cfg.CreateMap<Session, SessionViewModel>();
    cfg.CreateMap<SessionViewModel, Session>();

    cfg.CreateMap<DriverListViewModel, Driver>();
    cfg.CreateMap<Driver, DriverListViewModel>();


    //External Api
    cfg.CreateMap<CarDataDto, CarData>();
    cfg.CreateMap<CarData, CarDataDto>();

    cfg.CreateMap<Meeting, MeetingDto>();
    cfg.CreateMap<MeetingDto, Meeting>();

    cfg.CreateMap<Session, SessionDto>();
    cfg.CreateMap<SessionDto, Session>();

    cfg.CreateMap<DriverDto,  Driver>();
    cfg.CreateMap<Driver, DriverDto>();
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
