using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;
using Swashbuckle.AspNetCore.Swagger;
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
            builder.Services.AddSwaggerGen(options =>
            {
                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "WebApi.xml"));
                options.OperationFilter<ControllerErrorResponseDocumentationFilter>();
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
                Assert.That(operation.Responses["400"].Content["application/problem+json"], Is.Not.Null);
                Assert.That(operation.Responses.ContainsKey("500"), Is.False);
            }
        }

        [TestCase("/api/Meeting/all-tracks/{year}", OperationType.Get, "MeetingDataResponse")]
        [TestCase("/api/Session/insert-sessions", OperationType.Post, "ErrorViewModel")]
        public void Swagger_PreservesStructuredAndStringErrors(string path, OperationType method, string expectedSchema)
        {
            var response = _document.Paths[path].Operations[method].Responses["400"];

            Assert.That(response.Content["application/json"].Schema.AnyOf.Select(schema => schema.Reference?.Id ?? schema.Type),
                Is.EquivalentTo(new[] { expectedSchema, "string" }));
            Assert.That(response.Content["text/plain"].Schema.Type, Is.EqualTo("string"));
            Assert.That(response.Content["application/problem+json"].Schema.Reference.Id, Is.EqualTo("ValidationProblemDetails"));
        }

        [Test]
        public void Swagger_MeetingAndSession_UseActualResponseAndBodyModels()
        {
            var meeting = _document.Paths["/api/Meeting/{meetingKey}"].Operations[OperationType.Get];
            var session = _document.Paths["/api/Session/{sessionKey}"].Operations[OperationType.Get];
            var insert = _document.Paths["/api/Session/insert-sessions"].Operations[OperationType.Post];

            Assert.That(meeting.Responses["200"].Content["application/json"].Schema.Reference.Id, Is.EqualTo("MeetingViewModel"));
            Assert.That(meeting.Responses["404"].Content["text/plain"].Schema.Type, Is.EqualTo("string"));
            Assert.That(session.Responses["200"].Content["application/json"].Schema.Reference.Id, Is.EqualTo("SuccessViewModel"));
            Assert.That(session.Parameters.Single(parameter => parameter.Name == "meetingkey").In, Is.EqualTo(ParameterLocation.Query));
            Assert.That(session.Parameters.Single(parameter => parameter.Name == "sessionKey").In, Is.EqualTo(ParameterLocation.Path));
            Assert.That(insert.RequestBody.Description, Does.Contain("meeting_key"));
            Assert.That(_document.Components.Schemas["SessionMeetingKeyViewModel"].Properties.Keys, Does.Contain("meeting_key"));
        }

        [Test]
        public void Swagger_CarDataAndLaps_DistinguishProblemDetailsFromServiceErrors()
        {
            var carData = _document.Paths["/api/CarData/high-speed"].Operations[OperationType.Get];
            Assert.That(carData.Responses["404"].Content["application/problem+json"].Schema.Reference.Id, Is.EqualTo("ProblemDetails"));
            Assert.That(carData.Responses["400"].Content["application/problem+json"].Schema.AnyOf.Select(schema => schema.Reference.Id),
                Is.EquivalentTo(new[] { "ProblemDetails", "ValidationProblemDetails" }));

            foreach (var path in new[] { "/api/Laps/fast_lap_session", "/api/Laps/fast_sectors_session" })
            {
                var operation = _document.Paths[path].Operations[OperationType.Get];
                Assert.That(operation.Responses["400"].Content.Keys, Is.EquivalentTo(new[] { "application/problem+json" }));
                Assert.That(operation.Responses["404"].Content["application/json"].Schema.Reference.Id, Is.EqualTo("ErrorViewModel"));
            }
        }

        [Test]
        public void Swagger_SessionResult_DescribesExistingHttpAndPayloadStatusMismatch()
        {
            var operation = _document.Paths["/api/SessionResult/result"].Operations[OperationType.Get];
            Assert.That(operation.Responses["400"].Description, Does.Contain("status_code 404"));
            Assert.That(operation.Responses["404"].Description, Does.Contain("status_code 400"));
        }
    }
}
