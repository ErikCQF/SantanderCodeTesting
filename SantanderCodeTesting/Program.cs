using Polly;
using Polly.Extensions.Http;
using SantanderCodeTesting.InfraStructure;
using SantanderCodeTesting.Services;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// Add Infra Structure App Services

builder.Services.AddSingleton<IHackerService, HackerService>();

builder.Services.Configure<HackerServiceSettings>(
    builder.Configuration.GetSection(nameof(HackerServiceSettings)));

builder.Services.Configure<HttpClientSettings>(
    builder.Configuration.GetSection(nameof(HttpClientSettings)));

var httpClientSettings = builder.Configuration
    .GetSection(nameof(HttpClientSettings))
    .Get<HttpClientSettings>() ?? new HttpClientSettings();


var baseUrl = new Uri(httpClientSettings.BaseUrl);
builder.Services.AddHttpClient<IFetcher, FetcherHttpClient>(client =>
{
    client.BaseAddress = baseUrl;
    client.Timeout = TimeSpan.FromSeconds(10);
})

.ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
{
    PooledConnectionLifetime = TimeSpan.FromMinutes(10),
    MaxConnectionsPerServer = 50
})
.AddPolicyHandler(GetRetryPolicy())
.AddPolicyHandler(GetCircuitBreakerPolicy());



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();


static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError() // 5xx and 408
        .OrResult(msg => msg.StatusCode == HttpStatusCode.TooManyRequests)
        .WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: attempt =>
                TimeSpan.FromMilliseconds(200 * Math.Pow(2, attempt)) +
                TimeSpan.FromMilliseconds(Random.Shared.Next(0, 100))); // jitter
}

static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .CircuitBreakerAsync(
            handledEventsAllowedBeforeBreaking: 5,
            durationOfBreak: TimeSpan.FromSeconds(30));
}