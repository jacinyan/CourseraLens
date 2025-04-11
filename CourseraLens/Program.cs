using System.Diagnostics;
using CourseraLens.Models;
using CourseraLens.Swagger;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ===== Services =====
// Check at Model binding
builder.Services.AddControllers(options =>
{
    options.ModelBindingMessageProvider.SetValueIsInvalidAccessor(
        x => $"The value '{x}' is invalid.");
    options.ModelBindingMessageProvider.SetValueMustBeANumberAccessor(
        x => $"The field {x} must be a number.");
    options.ModelBindingMessageProvider.SetAttemptedValueIsInvalidAccessor(
        (x, y) => $"The value '{x}' is not valid for {y}.");
    options.ModelBindingMessageProvider.SetMissingKeyOrValueAccessor(
        () => "A value is required.");

    options.CacheProfiles.Add("NoCache",
        new CacheProfile { NoStore = true });
    options.CacheProfiles.Add("Any-60",
        new CacheProfile
        {
            Location = ResponseCacheLocation.Any,
            Duration = 60
        });
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.ParameterFilter<SortColumnFilter>();
    options.ParameterFilter<SortOrderFilter>();
});

// Cors
builder.Services.AddCors(options =>
{
    // Default
    options.AddDefaultPolicy(cfg =>
    {
        cfg.WithOrigins(builder.Configuration["AllowedOrigins"]!);
        cfg.AllowAnyHeader();
        cfg.AllowAnyMethod();
    });

    // Custom 
    options.AddPolicy("AnyOrigin", cfg =>
    {
        cfg.AllowAnyOrigin();
        cfg.AllowAnyHeader();
        cfg.AllowAnyMethod();
    });
});

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Suppress ModelState invalid filter globally
// builder.Services.Configure<ApiBehaviorOptions>(options =>
//     options.SuppressModelStateInvalidFilter = true);

// Server-side caching
builder.Services.AddResponseCaching(
    options =>
    {
        // Prevent memory shortage
        options.MaximumBodySize = 32 * 1024 * 1024;
        options.SizeLimit = 50 * 1024 * 1024;
    }
);

builder.Services.AddMemoryCache();

// ===== Build =====
var app = builder.Build();

// Built-in Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (app.Configuration.GetValue<bool>("UseDeveloperExceptionPage"))
    app.UseDeveloperExceptionPage();
else
    app.UseExceptionHandler("/error");
//app.UseExceptionHandler(action => {
//    action.Run(async context =>
//    {
//        var exceptionHandler =
//            context.Features.Get<IExceptionHandlerPathFeature>();

//        var details = new ProblemDetails();
//        details.Detail = exceptionHandler?.Error.Message;
//        details.Extensions["traceId"] =
//            System.Diagnostics.Activity.Current?.Id 
//              ?? context.TraceIdentifier;
//        details.Type =
//            "https://tools.ietf.org/html/rfc7231#section-6.6.1";
//        details.Status = StatusCodes.Status500InternalServerError;
//        await context.Response.WriteAsync(
//            System.Text.Json.JsonSerializer.Serialize(details));
//    });
//});

app.UseHttpsRedirection();
app.UseCors();
app.UseResponseCaching();
app.UseAuthorization();

// Custom middleware
app.Use((context, next) =>
{
    // A default cache-control directive when Cache-Profile is not present (although global, not to replace standard cache settings) 
    context.Response.Headers["cache-control"] =
        "no-cache, no-store";
    return next.Invoke();
});

// ===== Endpoints  =====
app.MapGet("/error",
    [EnableCors("AnyOrigin")] [ResponseCache(NoStore = true)]
    (HttpContext context) =>
    {
        var exceptionHandler =
            context.Features.Get<IExceptionHandlerPathFeature>();
        // TODO: logging, sending notifications, and more

        var details = new ProblemDetails
        {
            Detail = exceptionHandler?.Error.Message,
            Extensions =
            {
                ["traceid"] = Activity.Current?.Id
                              ?? context.TraceIdentifier
            },
            Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            Status = StatusCodes.Status500InternalServerError
        };

        return Results.Problem(details);
    });
app.MapGet("/error/test",
    [EnableCors("AnyOrigin")] [ResponseCache(NoStore = true)]
    () => { throw new Exception("test"); }
);
// app.MapGet("/cache/test/1",
//     [EnableCors("AnyOrigin")]
//     (HttpContext context) =>
//     {
//         context.Response.GetTypedHeaders().CacheControl = 
//         new Microsoft.Net.Http.Headers.CacheControlHeaderValue()
//         {
//             NoCache = true, 
//             NoStore = true 
//         }; 
//         return Results.Ok();
//     });

// app.MapGet("/cache/test/2",
//     [EnableCors("AnyOrigin")]
//     (HttpContext context) =>
//     {
//         return Results.Ok();
//     });
app.MapControllers();

app.Run();