using CourseraLens.Models;
using CourseraLens.Swagger;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ===== Services =====
builder.Services.AddControllers(options => {
    options.ModelBindingMessageProvider.SetValueIsInvalidAccessor(
        (x) => $"The value '{x}' is invalid.");
    options.ModelBindingMessageProvider.SetValueMustBeANumberAccessor(
        (x) => $"The field {x} must be a number.");
    options.ModelBindingMessageProvider.SetAttemptedValueIsInvalidAccessor(
        (x, y) => $"The value '{x}' is not valid for {y}.");
    options.ModelBindingMessageProvider.SetMissingKeyOrValueAccessor(
        () => $"A value is required.");
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

// Suppress ModelState Invalid Filter
builder.Services.Configure<ApiBehaviorOptions>(options =>
    options.SuppressModelStateInvalidFilter = true);

// ===== Build =====
var app = builder.Build();

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (app.Configuration.GetValue<bool>("UseDeveloperExceptionPage"))
    app.UseDeveloperExceptionPage();
else
    app.UseExceptionHandler("/error");

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();

// ===== Endpoints  =====
app.MapGet("/error",
    [EnableCors("AnyOrigin")] [ResponseCache(NoStore = true)]
    () =>
        Results.Problem());
app.MapControllers();

app.Run();