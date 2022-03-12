using Microsoft.AspNetCore.Http;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddHttpClient("name")
    .AddHttpMessageHandler<HttpClientMiddLeware>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.Use(async (context, next) =>
{
    // Do work that doesn't write to the Response. 
    await next.Invoke(context);
    // Do logging or other work that doesn't write to the Response.
});

app.UseMiddleware<CustomerMiddLeware>();

app.UseEndpoints(endpoints =>
{
    endpoints.MapGet("/user-name", async context =>
    {
        await context.Response.WriteAsync("vcluopeng1");
    });
});

app.MapGet("/user", async (context) =>
{
    await context.Response.WriteAsync("vcluopeng2");
});

app.MapWhen(context => context.Request.Query.ContainsKey("name"), (IApplicationBuilder app) =>
{
    app.Run(async context =>
    {
        var branchVer = context.Request.Query["branch"];
        await context.Response.WriteAsync($"Branch used = {branchVer}");
    });
});

app.MapRazorPages();

app.Run(async context =>
{
    await context.Response.WriteAsync("Hello from non-Map delegate.");
});

app.Run();

/// <summary>
/// 自定义中间件
/// </summary>
public class CustomerMiddLeware
{
    private readonly RequestDelegate _next;
    public CustomerMiddLeware(RequestDelegate next)
    {
        _next = next;
    }
    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.Headers.TryAdd("user-name", "vcluopeng");
        await _next(context);
    }
}

public class HttpClientMiddLeware : DelegatingHandler
{
    private readonly ILogger _logger;
    public HttpClientMiddLeware(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }
        request.Headers.Add("user-name", "vcluopeng");
        var response = await base.SendAsync(request, cancellationToken);
        return response;
    }
}