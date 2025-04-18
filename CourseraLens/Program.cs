using System.Data;
using System.Diagnostics;
using System.Text;
using CourseraLens.Constants;
using CourseraLens.GraphQl;
using CourseraLens.gRPC;
using CourseraLens.Models;
using CourseraLens.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Sinks.MSSqlServer;
using HotChocolate.AspNetCore;
using HotChocolate.Data;

var builder = WebApplication.CreateBuilder(args);
builder.Logging
    .ClearProviders()
    .AddSimpleConsole()
    .AddDebug();

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

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
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

// SqlServer
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"))
);

// GraphQLServer
builder.Services.AddGraphQLServer() 
    .AddAuthorization() 
    .AddQueryType<Query>() 
    .AddMutationType<Mutation>() 
    .AddProjections() 
    .AddFiltering() 
    .AddSorting();

// grpc
builder.Services.AddGrpc();

builder.Services.AddIdentity<ApiUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = 12;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme =
        options.DefaultChallengeScheme =
            options.DefaultForbidScheme =
                options.DefaultScheme =
                    options.DefaultSignInScheme =
                        options.DefaultSignOutScheme =
                            JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["JWT:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["JWT:Audience"],
        ValidateIssuerSigningKey = true,

        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(
                builder.Configuration["JWT:SigningKey"] ??
                string.Empty))
    };
});


// Suppress ModelState invalid filter globally
// builder.Services.Configure<ApiBehaviorOptions>(options =>
//     options.SuppressModelStateInvalidFilter = true);

// Response caching
builder.Services.AddResponseCaching(
    options =>
    {
        // Prevent memory shortage
        options.MaximumBodySize = 32 * 1024 * 1024;
        options.SizeLimit = 50 * 1024 * 1024;
    }
);
// In-memory cache
builder.Services.AddMemoryCache();
// Redis
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration =
        builder.Configuration["Redis:ConnectionString"];
});

// 
builder.Host.UseSerilog((ctx, lc) =>
    {
        lc.ReadFrom.Configuration(ctx.Configuration);
        lc.WriteTo.MSSqlServer(
            ctx.Configuration.GetConnectionString("DefaultConnection"),
            new MSSqlServerSinkOptions
            {
                TableName = "LogEvents",
                AutoCreateSqlTable = true
            },
            columnOptions: new ColumnOptions
            {
                AdditionalColumns = new[]
                {
                    new SqlColumn
                    {
                        ColumnName = "SourceContext",
                        PropertyName = "SourceContext",
                        DataType = SqlDbType.NVarChar
                    }
                }
            }
        );
    },
    writeToProviders: true);

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
app.UseAuthentication();
app.UseAuthorization();
app.MapGraphQL();
app.MapGrpcService<GrpcService>();

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

        // 
        app.Logger.LogError(
            CustomLogEvents.ErrorGet,
            "An unhandled exception occurred.");

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

// app.MapGet("/auth/test/1",
//     [Authorize]
//     [EnableCors("AnyOrigin")]
//     [ResponseCache(NoStore = true)] () => Results.Ok("You are authorized!"));
app.MapGet("/auth/test/2",
    [Authorize(Roles = RoleNames.Curator)]
    [EnableCors("AnyOrigin")]
    [ResponseCache(NoStore = true)]
    () => Results.Ok("You are authorized!"));
app.MapGet("/auth/test/3",
    [Authorize(Roles = RoleNames.Admin)]
    [EnableCors("AnyOrigin")]
    [ResponseCache(NoStore = true)]
    () => Results.Ok("You are authorized!"));
app.MapControllers();

app.Run();