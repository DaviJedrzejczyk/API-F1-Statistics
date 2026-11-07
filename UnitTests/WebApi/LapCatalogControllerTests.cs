using AutoMapper;
using Entities.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Services.Interfaces;
using Services.Models;
using Shared.Responses;
using WebApi.Controllers;
using WebApi.ViewModels;

namespace UnitTests.WebApi;

[TestFixture]
public sealed class LapCatalogControllerTests
{
    [Test]
    public void Controllers_HaveUnambiguousActivation_WithoutChangingLegacyDependencies()
    {
        ServiceCollection services = new();
        services.AddSingleton(Mock.Of<ILapService>());
        services.AddSingleton(Mock.Of<ILapFastSectorService>());
        services.AddSingleton(Mock.Of<IMapper>());
        services.AddSingleton(Mock.Of<ILapCatalogService>());
        using ServiceProvider provider = services.BuildServiceProvider();

        LapsController legacy = ActivatorUtilities.CreateInstance<LapsController>(provider);
        LapCatalogController catalog = ActivatorUtilities.CreateInstance<LapCatalogController>(provider);

        Assert.That(legacy, Is.Not.Null);
        Assert.That(catalog, Is.Not.Null);
    }

    [Test]
    public async Task Import_BusinessFailureUsesErrorWrapperAndItsHttpStatus()
    {
        Mock<ILapCatalogService> service = new();
        service.Setup(service => service.GetCatalog(100, It.IsAny<int[]>(), null, null, null, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResponseFactory.CreateInstance().CreateFailureSingleResponse<LapCatalogDto>(
                new LapCatalogException(502, "Invalid source lap identities.")));
        LapCatalogController controller = new(service.Object);

        IActionResult response = await controller.Import(100, new LapCatalogRequest { DriverNumbers = [1, 2, 3] }, default);

        Assert.That(response, Is.TypeOf<ObjectResult>());
        ObjectResult error = (ObjectResult)response;
        Assert.That(error.StatusCode, Is.EqualTo(502));
        Assert.That(error.Value, Is.TypeOf<ErrorViewModel>());
    }

    [Test]
    public async Task Import_UnexpectedFailureDoesNotExposeExceptionDetails()
    {
        Mock<ILapCatalogService> service = new();
        service.Setup(service => service.GetCatalog(100, It.IsAny<int[]>(), null, null, null, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResponseFactory.CreateInstance().CreateFailureSingleResponse<LapCatalogDto>(
                new InvalidOperationException("private database connection details")));
        LapCatalogController controller = new(service.Object);

        IActionResult response = await controller.Import(100, new LapCatalogRequest { DriverNumbers = [1, 2, 3] }, default);

        ObjectResult error = (ObjectResult)response;
        Assert.That(error.StatusCode, Is.EqualTo(503));
        Assert.That(System.Text.Json.JsonSerializer.Serialize(error.Value), Does.Not.Contain("private database"));
    }
}
