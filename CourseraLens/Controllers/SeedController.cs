using System.Globalization;
using CourseraLens.Models;
using CourseraLens.Models.CSV;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CourseraLens.Controllers;

[Authorize]
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
    [ResponseCache(CacheProfileName = "NoCache")]
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

        // Get existing course IDs and tag names for checking duplicates
        var existingCourseIds = await _context.Courses
            .Select(c => c.Id)
            .ToListAsync();

        var existingTagsDict = await _context.Tags
            .ToDictionaryAsync(t => t.TagName, t => t.Id);

        var now = DateTime.Now;
        var skippedRows = 0;
        var coursesAdded = 0;
        var tagsAdded = 0;
        var relationshipsAdded = 0;

        // Process records from CSV
        var records = csv.GetRecords<CourseRecord>().ToList();

        // Step 1: Extract all unique tag names from the CSV
        var allTagNames =
            new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
        foreach (var record in records)
            if (!string.IsNullOrEmpty(record.Tags))
                foreach (var tagName in record.Tags
                             .Split(';', StringSplitOptions.TrimEntries)
                             .Distinct(
                                 StringComparer.InvariantCultureIgnoreCase))
                    if (!string.IsNullOrEmpty(tagName) &&
                        !existingTagsDict.ContainsKey(tagName))
                        allTagNames.Add(tagName);

        // Step 2: Insert new tags and get their IDs
        foreach (var tagName in allTagNames)
            try
            {
                var tagId = await _context.Database.ExecuteSqlRawAsync(
                    "INSERT INTO Tags (TagName, CreatedDate, LastModifiedDate) VALUES ({0}, {1}, {2}); " +
                    "SELECT SCOPE_IDENTITY();",
                    tagName, now, now);

                // Use a separate query to get the ID of the inserted tag
                var insertedTag = await _context.Tags
                    .Where(t => t.TagName == tagName)
                    .FirstOrDefaultAsync();

                if (insertedTag != null)
                {
                    existingTagsDict[tagName] = insertedTag.Id;
                    tagsAdded++;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inserting tag: {TagName}", tagName);
            }

        // Step 3: Insert courses with IDENTITY_INSERT ON
        using (var transaction = _context.Database.BeginTransaction())
        {
            try
            {
                _context.Database.ExecuteSqlRaw(
                    "SET IDENTITY_INSERT Courses ON");

                foreach (var record in records)
                {
                    if (!record.Id.HasValue ||
                        string.IsNullOrEmpty(record.Title) ||
                        existingCourseIds.Contains(record.Id.Value))
                    {
                        skippedRows++;
                        continue;
                    }

                    try
                    {
                        await _context.Database.ExecuteSqlRawAsync(
                            "INSERT INTO Courses (Id, Title, Organization, CertType, Rating, Difficulty, StudentsEnrolled, CreatedDate, LastModifiedDate) " +
                            "VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8})",
                            record.Id.Value,
                            record.Title,
                            record.Organization ?? string.Empty,
                            record.CertType ?? string.Empty,
                            record.Rating ?? 0,
                            record.Difficulty ?? string.Empty,
                            record.StudentsEnrolled ?? 0,
                            now,
                            now);

                        existingCourseIds.Add(record.Id.Value);
                        coursesAdded++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,
                            "Error inserting course: {CourseId}, {Title}",
                            record.Id, record.Title);
                    }
                }

                _context.Database.ExecuteSqlRaw(
                    "SET IDENTITY_INSERT Courses OFF");
                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                _logger.LogError(ex, "Error during course insertion");
                return StatusCode(500, ex.Message);
            }
        }

        // Step 4: Insert course-tag relationships
        foreach (var record in records)
        {
            if (!record.Id.HasValue ||
                string.IsNullOrEmpty(record.Title) ||
                string.IsNullOrEmpty(record.Tags) ||
                !existingCourseIds.Contains(record.Id.Value))
                continue;

            foreach (var tagName in record.Tags
                         .Split(';', StringSplitOptions.TrimEntries)
                         .Distinct(StringComparer.InvariantCultureIgnoreCase))
            {
                if (string.IsNullOrEmpty(tagName) ||
                    !existingTagsDict.ContainsKey(tagName)) continue;

                try
                {
                    await _context.Database.ExecuteSqlRawAsync(
                        "IF NOT EXISTS (SELECT 1 FROM CoursesTags WHERE CourseId = {0} AND TagId = {1}) " +
                        "INSERT INTO CoursesTags (CourseId, TagId, CreatedDate) VALUES ({0}, {1}, {2})",
                        record.Id.Value,
                        existingTagsDict[tagName],
                        now);

                    relationshipsAdded++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Error inserting course-tag relationship: CourseId={CourseId}, TagName={TagName}",
                        record.Id.Value, tagName);
                }
            }
        }

        return new JsonResult(new
        {
            CoursesAdded = coursesAdded,
            TagsAdded = tagsAdded,
            RelationshipsAdded = relationshipsAdded,
            SkippedRows = skippedRows
        });
    }
}