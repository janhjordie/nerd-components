using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Options;
using TheNerdCollective.Integrations.Brevo;
using Xunit;

namespace TheNerdCollective.Integrations.Brevo.Tests;

public sealed class BrevoServiceTests
{
    [Fact]
    public async Task CreateOrUpdateContactAndGetIdAsync_posts_contact_and_returns_id()
    {
        string? requestBody = null;
        var handler = new StubHttpMessageHandler(async (request, cancellationToken) =>
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal("/v3/contacts", request.RequestUri?.AbsolutePath);
            requestBody = await request.Content!.ReadAsStringAsync(cancellationToken);
            return JsonResponse("""{"id":42}""", HttpStatusCode.Created);
        });
        BrevoService service = CreateService(handler);

        var attributes = new Dictionary<string, string>
        {
            ["FIRSTNAME"] = "Guest",
            ["LASTNAME"] = "Example",
            ["SMS"] = "+4528876905",
            ["CONTACT_TIMEZONE"] = "Europe/Copenhagen"
        };
        BrevoDataResult<long> result = await service.CreateOrUpdateContactAndGetIdAsync(
            " guest@example.com ",
            attributes,
            [12]);

        Assert.True(result.Succeeded);
        Assert.Equal(42L, result.Data);
        Assert.Equal((int)HttpStatusCode.Created, result.StatusCode);

        using JsonDocument body = JsonDocument.Parse(requestBody!);
        Assert.Equal("guest@example.com", body.RootElement.GetProperty("email").GetString());
        Assert.True(body.RootElement.GetProperty("updateEnabled").GetBoolean());
        Assert.True(body.RootElement.GetProperty("getId").GetBoolean());
        Assert.Equal(12L, body.RootElement.GetProperty("listIds")[0].GetInt64());
        JsonElement contactAttributes = body.RootElement.GetProperty("attributes");
        Assert.Equal("Guest", contactAttributes.GetProperty("FIRSTNAME").GetString());
        Assert.Equal("Example", contactAttributes.GetProperty("LASTNAME").GetString());
        Assert.Equal("+4528876905", contactAttributes.GetProperty("SMS").GetString());
        Assert.Equal("Europe/Copenhagen", contactAttributes.GetProperty("CONTACT_TIMEZONE").GetString());
    }

    [Fact]
    public async Task CreateOrUpdateContactAndGetIdAsync_looks_up_id_after_update_without_response_body()
    {
        int requestCount = 0;
        var handler = new StubHttpMessageHandler((request, _) =>
        {
            requestCount++;
            if (requestCount == 1)
            {
                Assert.Equal(HttpMethod.Post, request.Method);
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NoContent));
            }

            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal("/v3/contacts/guest%40example.com", request.RequestUri?.AbsolutePath);
            return Task.FromResult(JsonResponse("""{"id":57}"""));
        });
        BrevoService service = CreateService(handler);

        BrevoDataResult<long> result = await service.CreateOrUpdateContactAndGetIdAsync("guest@example.com");

        Assert.True(result.Succeeded);
        Assert.Equal(57L, result.Data);
        Assert.Equal((int)HttpStatusCode.NoContent, result.StatusCode);
        Assert.Equal(2, requestCount);
    }

    [Fact]
    public async Task CreateContactNoteAsync_posts_note_linked_to_contact()
    {
        string? requestBody = null;
        var handler = new StubHttpMessageHandler(async (request, cancellationToken) =>
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal("/v3/crm/notes", request.RequestUri?.AbsolutePath);
            requestBody = await request.Content!.ReadAsStringAsync(cancellationToken);
            return JsonResponse("""{"id":"note-1"}""");
        });
        BrevoService service = CreateService(handler);

        BrevoResult result = await service.CreateContactNoteAsync(42, "A message from the contact form.");

        Assert.True(result.Succeeded);
        Assert.Equal((int)HttpStatusCode.OK, result.StatusCode);
        using JsonDocument body = JsonDocument.Parse(requestBody!);
        Assert.Equal("A message from the contact form.", body.RootElement.GetProperty("text").GetString());
        Assert.Equal(42L, body.RootElement.GetProperty("contactIds")[0].GetInt64());
    }

    private static BrevoService CreateService(HttpMessageHandler handler)
    {
        var client = new HttpClient(handler);
        var options = Options.Create(new BrevoOptions
        {
            ApiKey = "test-api-key",
            BaseUrl = "https://brevo.test/v3/"
        });
        return new BrevoService(client, options);
    }

    private static HttpResponseMessage JsonResponse(
        string json,
        HttpStatusCode statusCode = HttpStatusCode.OK) =>
        new(statusCode)
        {
            Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
        };

    private sealed class StubHttpMessageHandler(
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            handler(request, cancellationToken);
    }
}
