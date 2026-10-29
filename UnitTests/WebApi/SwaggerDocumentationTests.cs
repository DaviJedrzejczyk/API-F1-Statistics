using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;
using Swashbuckle.AspNetCore.Swagger;
using WebApi.Config;
using WebApi.Controllers.Meetings;

namespace UnitTests.WebApi
{
    [TestFixture]
    public class SwaggerDocumentationTests
    {
        private OpenApiDocument _document = null!;

        [OneTimeSetUp]
        public async Task GenerateSwagger()
        {
            // Discover the real controllers without registering business services or background workers.
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                ApplicationName = typeof(MeetingController).Assembly.GetName().Name
            });
            builder.Services.AddControllers();
            builder.Services.AddApiErrorContracts();
            builder.Services.AddSwaggerGen(options =>
            {
                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "WebApi.xml"));
            });

            await using var app = builder.Build();
            app.MapControllers();
            _document = app.Services.GetRequiredService<ISwaggerProvider>().GetSwagger("v1");

            var artifact = Path.Combine(TestContext.CurrentContext.WorkDirectory, "controller-swagger.json");
            using (var writer = File.CreateText(artifact))
                _document.SerializeAsV3(new OpenApiJsonWriter(writer));
            TestContext.AddTestAttachment(artifact, "Swagger generated without external API calls.");
        }

        [Test]
        public void Swagger_ContainsOnlyImplementedApiRoutes_WithXmlDescriptions()
        {
            Assert.That(_document.Paths.Keys, Is.EquivalentTo(new[]
            {
                "/api/CarData/high-speed",
                "/api/Driver/drivers-session",
                "/api/Laps/fast_lap_session",
                "/api/Laps/fast_sectors_session",
                "/api/Meeting/tracks-season",
                "/api/Meeting/{meetingKey}",
                "/api/Meeting/all-tracks/{year}",
                "/api/Overtake/get-overtakes-session/{sessionKey}",
                "/api/SessionResult/result",
                "/api/Session/insert-sessions",
                "/api/Session/{sessionKey}"
            }));

            foreach (var operation in _document.Paths.Values.SelectMany(path => path.Operations.Values))
            {
                Assert.That(operation.Summary, Is.Not.Null.And.Not.Empty);
                foreach (var parameter in operation.Parameters)
                    Assert.That(parameter.Description, Is.Not.Null.And.Not.Empty, parameter.Name);
                Assert.That(operation.Responses["400"].Content["application/json"].Schema.Reference.Id, Is.EqualTo("ErrorViewModel"));
                Assert.That(operation.Responses["500"].Content["application/json"].Schema.Reference.Id, Is.EqualTo("ErrorViewModel"));
            }
        }

        [Test]
        public void Swagger_ErrorResponsesUseErrorViewModelOnly()
        {
            var json = new StringWriter();
            _document.SerializeAsV3(new OpenApiJsonWriter(json));
            var text = json.ToString();

            Assert.That(text, Does.Not.Contain("ProblemDetails"));
            Assert.That(text, Does.Not.Contain("problem+json"));

            foreach (var operation in _document.Paths.Values.SelectMany(path => path.Operations.Values))
            {
                foreach (var (status, response) in operation.Responses.Where(pair => pair.Key != "200"))
                {
                    Assert.That(response.Content.ContainsKey("application/json"), status);
                    foreach (var media in response.Content.Values)
                        Assert.That(media.Schema.Reference.Id, Is.EqualTo("ErrorViewModel"), status);
                }
            }
        }

        [Test]
        public void Swagger_PostBodyOperations_DocumentUnsupportedMediaType()
        {
            var insert = _document.Paths["/api/Session/insert-sessions"].Operations[OperationType.Post];
            var tracks = _document.Paths["/api/Meeting/tracks-season"].Operations[OperationType.Post];

            foreach (var operation in new[] { insert, tracks })
                Assert.That(operation.Responses["415"].Content["application/json"].Schema.Reference.Id, Is.EqualTo("ErrorViewModel"));
        }

        [Test]
        public void Swagger_MeetingAndSession_UseActualResponseAndBodyModels()
        {
            var meeting = _document.Paths["/api/Meeting/{meetingKey}"].Operations[OperationType.Get];
            var session = _document.Paths["/api/Session/{sessionKey}"].Operations[OperationType.Get];
            var insert = _document.Paths["/api/Session/insert-sessions"].Operations[OperationType.Post];

            Assert.That(meeting.Responses["200"].Content["application/json"].Schema.Reference.Id, Is.EqualTo("MeetingViewModel"));
            Assert.That(meeting.Responses["404"].Content["application/json"].Schema.Reference.Id, Is.EqualTo("ErrorViewModel"));
            Assert.That(session.Responses["200"].Content["application/json"].Schema.Reference.Id, Is.EqualTo("SuccessViewModel"));
            Assert.That(session.Parameters.Single(parameter => parameter.Name == "meetingkey").In, Is.EqualTo(ParameterLocation.Query));
            Assert.That(session.Parameters.Single(parameter => parameter.Name == "sessionKey").In, Is.EqualTo(ParameterLocation.Path));
            Assert.That(insert.RequestBody.Description, Does.Contain("meeting_key"));
            Assert.That(_document.Components.Schemas["SessionMeetingKeyViewModel"].Properties.Keys, Does.Contain("meeting_key"));
        }

        [Test]
        public void Swagger_CarDataAndLaps_DocumentErrorViewModelForEveryFailure()
        {
            var carData = _document.Paths["/api/CarData/high-speed"].Operations[OperationType.Get];
            Assert.That(carData.Responses["404"].Content["application/json"].Schema.Reference.Id, Is.EqualTo("ErrorViewModel"));
            Assert.That(carData.Responses["400"].Content["application/json"].Schema.Reference.Id, Is.EqualTo("ErrorViewModel"));

            foreach (var path in new[] { "/api/Laps/fast_lap_session", "/api/Laps/fast_sectors_session" })
            {
                var operation = _document.Paths[path].Operations[OperationType.Get];
                Assert.That(operation.Responses["400"].Content["application/json"].Schema.Reference.Id, Is.EqualTo("ErrorViewModel"));
                Assert.That(operation.Responses["404"].Content["application/json"].Schema.Reference.Id, Is.EqualTo("ErrorViewModel"));
            }
        }

        [Test]
        public void Swagger_SessionResult_DocumentsHttpStatusForEachServiceFailure()
        {
            var operation = _document.Paths["/api/SessionResult/result"].Operations[OperationType.Get];
            Assert.That(operation.Responses["400"].Description, Does.Contain("null list"));
            Assert.That(operation.Responses["404"].Description, Does.Contain("empty list"));
        }
    }
}
