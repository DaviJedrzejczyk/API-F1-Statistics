using System.Net;
using System.Text;

namespace IntegrationTests.Infrastructure;

public sealed class FixtureHttpHandler : HttpMessageHandler
{
    private readonly Queue<(string Path, string? Fixture, HttpStatusCode Status, bool Timeout)> responses = new();
    private readonly List<string> unexpected = new();
    public List<string> Requests { get; } = new();

    public void Enqueue(string path, string? fixture, HttpStatusCode status = HttpStatusCode.OK)
        => responses.Enqueue((path, fixture, status, false));

    public void EnqueueTimeout(string path) => responses.Enqueue((path, null, HttpStatusCode.OK, true));

    public void VerifyComplete()
    {
        Assert.That(unexpected, Is.Empty, "Unexpected external requests were attempted.");
        Assert.That(responses, Is.Empty, "Expected external request was not made.");
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var path = request.RequestUri!.PathAndQuery;
        Requests.Add(path);
        // Never falls through to a network handler, even for unexpected URLs.
        if (request.Method != HttpMethod.Get || request.RequestUri.Host != "fixtures.invalid" ||
            !responses.TryPeek(out var next) || path != next.Path)
        {
            unexpected.Add(path);
            throw new InvalidOperationException($"Unexpected external request: {path}");
        }
        responses.Dequeue();
        if (next.Timeout)
            return Task.FromException<HttpResponseMessage>(new TaskCanceledException("Deterministic external timeout."));
        var body = next.Fixture == null ? "" : File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", next.Fixture));
        return Task.FromResult(new HttpResponseMessage(next.Status)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        });
    }
}
