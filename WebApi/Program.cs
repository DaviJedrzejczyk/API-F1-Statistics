using Dao;
using ExternalApi.Impls;
using ExternalApi.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using WebApi.BackgroundServices;
using WebApi.Config;
using WebApi.Controllers.Constants;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddCors();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "WebApi.xml"));
    options.OperationFilter<ControllerErrorResponseDocumentationFilter>();
});

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

builder.Services.AddHttpClient<IF1ApiClient, F1ApiClient>(c => c.BaseAddress = new Uri(F1ApiURL.URL_API_F1));
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

builder.Services.AddHostedService<SessionUpdateWorker>();

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

/// <summary>
/// Documents alternative controller error bodies and automatic API validation responses.
/// </summary>
public sealed class ControllerErrorResponseDocumentationFilter : IOperationFilter
{
    /// <summary>
    /// Preserves all declared error types when multiple responses share an HTTP status.
    /// </summary>
    /// <param name="operation">OpenAPI operation to document.</param>
    /// <param name="context">Controller action and schema generation context.</param>
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.MethodInfo.DeclaringType?.IsDefined(typeof(ApiControllerAttribute), true) != true)
            return;

        // ApiExplorer keeps only one type per status; these actions also return strings or wrappers.
        var responses = context.MethodInfo.GetCustomAttributes<ProducesResponseTypeAttribute>()
            .Where(attribute => attribute.StatusCode >= 400 && attribute.StatusCode < 500)
            .Append(new ProducesResponseTypeAttribute(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest));

        foreach (var group in responses.GroupBy(attribute => attribute.StatusCode))
        {
            var status = group.Key.ToString();
            if (!operation.Responses.TryGetValue(status, out var response))
            {
                response = new OpenApiResponse { Description = "Invalid request input." };
                operation.Responses.Add(status, response);
            }

            response.Content.Clear();
            var types = group.Select(attribute => attribute.Type).Distinct().ToList();
            foreach (var mediaGroup in types.GroupBy(type => typeof(ProblemDetails).IsAssignableFrom(type)))
            {
                var schemas = mediaGroup.Select(type => context.SchemaGenerator.GenerateSchema(type, context.SchemaRepository)).ToList();
                var schema = schemas.Count == 1 ? schemas[0] : new OpenApiSchema { AnyOf = schemas };
                var mediaType = mediaGroup.Key ? "application/problem+json" : "application/json";
                response.Content[mediaType] = new OpenApiMediaType { Schema = schema };
                if (!mediaGroup.Key)
                    response.Content["text/json"] = new OpenApiMediaType { Schema = schema };
            }

            if (types.Contains(typeof(string)))
                response.Content["text/plain"] = new OpenApiMediaType { Schema = new OpenApiSchema { Type = "string" } };
        }
    }
}
