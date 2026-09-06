using AutoMapper;
using Dao;
using Dao.Impl;
using Dao.Interface;
using Entities;
using Entities.Dtos.OvertakeDTOs;
using Entities.Dtos.PitDTOs;
using ExternalApi.Impls;
using ExternalApi.Interfaces;
using ExternalApi.ViewModels;
using Microsoft.EntityFrameworkCore;
using Services.Impl;
using Services.Interfaces;
using WebApi.Controllers.Meetings;
using WebApi.ViewModels.DriversViews;
using WebApi.ViewModels.MeetingsViews;
using WebApi.ViewModels.SessionResultsViews;
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

    cfg.CreateMap<SessionResult, SessionResultListViewModel>();
    cfg.CreateMap<SessionResultListViewModel, SessionResultListViewModel>();

    //External Api / Entities
    cfg.CreateMap<CarDataDto, CarData>();
    cfg.CreateMap<CarData, CarDataDto>();

    cfg.CreateMap<Meeting, MeetingDto>();
    cfg.CreateMap<MeetingDto, Meeting>();

    cfg.CreateMap<Session, SessionDto>();
    cfg.CreateMap<SessionDto, Session>();

    cfg.CreateMap<DriverDto,  Driver>();
    cfg.CreateMap<Driver, DriverDto>();

    cfg.CreateMap<SessionResult, SessionResultDto>();
    cfg.CreateMap<SessionResultDto, SessionResult>();

    cfg.CreateMap<Pit, PitDto>();
    cfg.CreateMap<PitDto, Pit>();

    cfg.CreateMap<Overtake, OvertakeDto>();
    cfg.CreateMap<OvertakeDto, Overtake>();

    cfg.CreateMap<RaceControl, RaceControlDto>();
    cfg.CreateMap<RaceControlDto, RaceControl>();

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

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
