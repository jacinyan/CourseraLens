using System.Globalization;
using CourseraLens.Models;
using CourseraLens.Models.CSV;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CourseraLens.Controllers;

[Route("[controller]")]
[ApiController]
public class SeedController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<SeedController> _logger;

    public SeedController(
        ApplicationDbContext context,
        IWebHostEnvironment env,
        ILogger<SeedController> logger)
    {
        _context = context;
        _env = env;
        _logger = logger;
    }

    [HttpPut(Name = "Seed")]
    [ResponseCache(NoStore = true)]
    public async Task<IActionResult> Put()
    {
        var config = new CsvConfiguration(CultureInfo.GetCultureInfo("en-AU"))
        {
            HasHeaderRecord = true,
            Delimiter = ","
        };

        using var reader = new StreamReader(
            Path.Combine(_env.ContentRootPath, "Data/coursera_data.csv"));
        using var csv = new CsvReader(reader, config);

        var existingCourses = await _context.Courses
            .ToDictionaryAsync(c => c.Id);
        var existingTags = await _context.Tags
            .ToDictionaryAsync(t => t.TagName);

        var now = DateTime.Now;
        var skippedRows = 0;

        var records = csv.GetRecords<CourseRecord>();
        foreach (var record in records)
        {
            if (!record.Id.HasValue ||
                string.IsNullOrEmpty(record.Title) ||
                existingCourses.ContainsKey(record.Id.Value))
            {
                skippedRows++;
                continue;
            }

            var course = new Course
            {
                Id = record.Id.Value,
                Title = record.Title,
                Organization = record.Organization ?? string.Empty,
                CertType = record.CertType ?? string.Empty,
                Rating = record.Rating ?? 0,
                Difficulty = record.Difficulty ?? string.Empty,
                StudentsEnrolled = record.StudentsEnrolled ?? 0,
                CreatedDate = now,
                LastModifiedDate = now
            };
            _context.Courses.Add(course);

            if (!string.IsNullOrEmpty(record.Tags))
                foreach (var tagName in record.Tags
                             .Split(';', StringSplitOptions.TrimEntries)
                             .Distinct(
                                 StringComparer.InvariantCultureIgnoreCase))
                {
                    if (!existingTags.TryGetValue(tagName,
                            out var tag))
                    {
                        tag = new Tag
                        {
                            TagName = tagName,
                            CreatedDate = now,
                            LastModifiedDate = now
                        };
                        _context.Tags.Add(tag);
                        existingTags.Add(tagName, tag);
                    }

                    _context.CoursesTags.Add(new CoursesTags
                    {
                        Course = course,
                        Tag = tag,
                        CreatedDate = now
                    });
                }
        }

        await _context.SaveChangesAsync();

        return new JsonResult(new
        {
            Courses = _context.Courses.Count(),
            Tags = _context.Tags.Count(),
            SkippedRows = skippedRows
        });
    }
}