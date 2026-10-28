using System.Reflection;
using back_prueba.Data;
using back_prueba.Dtos;
using back_prueba.Middleware;
using back_prueba.Repositories;
using back_prueba.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// La cadena de conexión se lee de la configuración (appsettings / user-secrets / variables de entorno).
var connectionString = builder.Configuration.GetConnectionString("LabDb")
    ?? throw new InvalidOperationException("Falta la cadena de conexión 'ConnectionStrings:LabDb' en la configuración.");

builder.Services.AddDbContext<LabDbContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddScoped<ISampleRepository, SampleRepository>();
builder.Services.AddScoped<ISampleService, SampleService>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        // Los errores de validación de modelo usan la misma estructura ErrorResponse que el resto.
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(e => e.Value?.Errors.Count > 0)
                .ToDictionary(
                    e => e.Key,
                    e => e.Value!.Errors.Select(x => string.IsNullOrEmpty(x.ErrorMessage) ? "Valor inválido." : x.ErrorMessage).ToArray());

            return new BadRequestObjectResult(new ErrorResponse(
                StatusCodes.Status400BadRequest, "La solicitud tiene errores de validación.", errors));
        };
    });

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy => policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "API Recepción de Muestras", Version = "v1" });
    var xmlFile = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
    options.IncludeXmlComments(xmlFile);
});

var app = builder.Build();

app.UseMiddleware<RequestLoggingMiddleware>();
app.UseExceptionHandler();

app.UseSwagger();
app.UseSwaggerUI();

// En desarrollo el front llama por http://localhost:5159; redirigir a https rompería el preflight de CORS.
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseCors();
app.UseAuthorization();

app.MapControllers();

app.Run();
