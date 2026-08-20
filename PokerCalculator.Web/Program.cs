// Static-file host for the equity calculator's frontend (wwwroot), plus a reverse proxy
// for /api/* to PokerCalculator.Api. The browser only ever talks to this origin - the API
// itself stays bound to localhost and is never exposed publicly, so no CORS is needed.
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient("api", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5000");
});

var app = builder.Build();

app.Map("/api/{**path}", async (HttpContext context, IHttpClientFactory httpClientFactory, string path) =>
{
    var client = httpClientFactory.CreateClient("api");
    var targetUri = new Uri(client.BaseAddress!, $"/api/{path}{context.Request.QueryString}");

    using var proxyRequest = new HttpRequestMessage(new HttpMethod(context.Request.Method), targetUri);
    if (context.Request.ContentLength is > 0)
    {
        proxyRequest.Content = new StreamContent(context.Request.Body);
        if (context.Request.ContentType is string requestContentType)
        {
            proxyRequest.Content.Headers.TryAddWithoutValidation("Content-Type", requestContentType);
        }
    }

    using var proxyResponse = await client.SendAsync(
        proxyRequest, HttpCompletionOption.ResponseHeadersRead, context.RequestAborted);

    context.Response.StatusCode = (int)proxyResponse.StatusCode;
    if (proxyResponse.Content.Headers.ContentType is System.Net.Http.Headers.MediaTypeHeaderValue contentType)
    {
        context.Response.ContentType = contentType.ToString();
    }
    await proxyResponse.Content.CopyToAsync(context.Response.Body, context.RequestAborted);
});

app.UseDefaultFiles();
app.UseStaticFiles();

app.Run();
