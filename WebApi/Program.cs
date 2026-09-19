using Dao;
using ExternalApi.Impls;
using Microsoft.EntityFrameworkCore;
using WebApi.Config;


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
    cfg.AddProfile<AutoMapperConfig>();
});

builder.Services
    .AddApplicationServicesTransient();

builder.Services
    .AddControllers()
    .AddJsonConfiguration();

builder.Services.AddHttpClient<F1ApiClient>();
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
    op.AllowAnyMethod();
    op.AllowAnyHeader();
    op.AllowAnyOrigin();
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
