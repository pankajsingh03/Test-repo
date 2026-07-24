using Microsoft.OpenApi.Models;
using OrderService.Services;
using Polly;
using Polly.Extensions.Http;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "OrderService API",
        Version = "v1",
        Description = "In-memory order management API with payment integration."
    });
});

// Configure JWT authentication
var jwtKey = builder.Configuration["Jwt:Key"]!;
var jwtIssuer = builder.Configuration["Jwt:Issuer"]!;
var jwtAudience = builder.Configuration["Jwt:Audience"]!;
var keyBytes = Encoding.UTF8.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes)
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddHttpClient<IPaymentServiceClient, PaymentServiceClient>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5024/");
})
    .AddPolicyHandler((sp, request) =>
    {
        var logger = sp.GetRequiredService<ILogger<PaymentServiceClient>>();

        var timeout = Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(5));

        var retry = HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: _ => TimeSpan.FromSeconds(2),
                onRetry: (outcome, timespan, retryAttempt, context) =>
                {
                    logger.LogWarning("PaymentService call failed. Waiting {Delay} before retry {Retry}. Reason: {Reason}", timespan, retryAttempt, outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString());
                }
            );

        var circuit = HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 3,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (outcome, breakDelay) =>
                {
                    logger.LogWarning("Circuit opened for {Delay} due to {Reason}", breakDelay, outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString());
                },
                onReset: () => logger.LogInformation("Circuit closed - operations restored."),
                onHalfOpen: () => logger.LogInformation("Circuit is half-open; testing the downstream service.")
            );

        // Order: retry -> circuit -> timeout (timeout is innermost so each attempt has its own timeout)
        return Policy.WrapAsync(retry, circuit, timeout);
    });

builder.Services.AddSingleton<IOrderService, OrderService.Services.OrderService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.RoutePrefix = string.Empty;
    options.SwaggerEndpoint("swagger/v1/swagger.json", "OrderService API V1");
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/", () => Results.Redirect("/swagger"));
app.Run();
