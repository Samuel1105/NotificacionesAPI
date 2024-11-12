using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using NotificacionesAPI.Hubs;
using NotificacionesAPI.Services;
using NotificacionesAPI.Middleware;
using NotificacionesAPI.Config;
using System.Text;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);


// Servicios base
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configuraci�n de SignalR con opciones espec�ficas
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
    options.MaximumReceiveMessageSize = 32 * 1024;
    options.HandshakeTimeout = TimeSpan.FromSeconds(15);
});

// Registrar servicios personalizados
builder.Services.AddSingleton<IApiKeyValidationService, ApiKeyValidationService>();

// Configuraci�n de CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("NotificationPolicy", builder =>
    {
        builder
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()
            .SetIsOriginAllowed(_ => true); // La validaci�n espec�fica se hace en el middleware
    });
});

// Logging
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.AddDebug();
});

// Construir la aplicaci�n
var app = builder.Build();

// Pipeline de middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // Middleware personalizado para logging en desarrollo
    app.Use(async (context, next) =>
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogInformation($"Request: {context.Request.Method} {context.Request.Path}");
        await next();
    });
}

// Middleware de seguridad
app.UseHttpsRedirection();
app.UseStaticFiles(); // Si necesitas servir archivos est�ticos

// CORS debe ir antes de la autenticaci�n
app.UseCors("NotificationPolicy");

// Middleware personalizado de autenticaci�n por API Key
app.UseMiddleware<ApiKeyAuthMiddleware>();

// Middleware de autorizaci�n
app.UseAuthentication();



// Endpoints de la API
app.MapControllers();

// Configuraci�n del hub de SignalR
app.MapHub<NotificationHub>("/notificationHub", options =>
{
    options.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransportType.WebSockets |
                        Microsoft.AspNetCore.Http.Connections.HttpTransportType.LongPolling;
});

// Middleware de manejo de errores global
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";

        var error = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        if (error != null)
        {
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogError(error.Error, "Unhandled exception");

            await context.Response.WriteAsJsonAsync(new
            {
                StatusCode = 500,
                Message = app.Environment.IsDevelopment() ? error.Error.Message : "An internal error occurred"
            });
        }
    });
});

// Iniciar la aplicaci�n
await app.RunAsync();