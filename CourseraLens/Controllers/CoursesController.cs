using System.ComponentModel.DataAnnotations;
using System.Linq.Dynamic.Core;
using CourseraLens.Attributes;
using CourseraLens.DTO;
using CourseraLens.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CourseraLens.Controllers;

[Route("[controller]")]
[ApiController]
public class CoursesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CoursesController> _logger;

    public CoursesController(ApplicationDbContext context,
        ILogger<CoursesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet(Name = "GetCourses")]
    [ResponseCache(Location = ResponseCacheLocation.Any, Duration = 60)]
    public async Task<RestDto<Course[]>> Get(
            [FromQuery] RequestDto<CourseDto> input
    )
    {
        var query = _context.Courses.AsQueryable();
        if (!string.IsNullOrEmpty(input.FilterQuery))
            query = query.Where(b => b.Title.Contains(input.FilterQuery));
        var resultCount = await query.CountAsync();
        query = query
            .OrderBy($"{input.SortColumn} {input.SortOrder}")
            .Skip(input.PageIndex * input.PageSize)
            .Take(input.PageSize);

        return new RestDto<Course[]>
        {
            Data = await query.ToArrayAsync(),
            PageIndex =input.PageIndex,
            PageSize =input.PageSize,
            ResultCount = resultCount,
            Links = new List<LinkDto>
            {
                new(
                    Url.Action(null, "Courses", new { input.PageIndex, input.PageSize },
                        Request.Scheme)!, "self", "GET")
            }
        };
    }

    [HttpPost(Name = "UpdateCourse")]
    [ResponseCache(NoStore = true)]
    public async Task<RestDto<Course?>> Post(CourseDto model)
    {
        var course = await _context.Courses
            .Where(b => b.Id == model.Id)
            .FirstOrDefaultAsync();
        if (course != null)
        {
            if (!string.IsNullOrEmpty(model.Title))
                course.Title = model.Title;
            if (model.StudentsEnrolled is > 0)
                course.StudentsEnrolled = model.StudentsEnrolled.Value;
            course.LastModifiedDate = DateTime.Now;

            _context.Courses.Update(course);
            await _context.SaveChangesAsync();
        }

        ;
        return new RestDto<Course?>
        {
            Data = course,
            Links = new List<LinkDto>
            {
                new(
                    Url.Action(
                        null,
                        "Courses",
                        model,
                        Request.Scheme)!,
                    "self",
                    "POST")
            }
        };
    }

    [HttpDelete(Name ="DeleteCourse")]
    [ResponseCache(NoStore = true)]
    public async Task<RestDto<Course?>> Delete(int id)
    {
        var course = await _context.Courses
            .Where(b => b.Id == id)
            .FirstOrDefaultAsync();
        if (course != null)
        {
            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();
        }

        return new RestDto<Course?>
        {
            Data = course,
            Links = new List<LinkDto>
            {
                new(
                    Url.Action(
                        null,
                        "Courses",
                        new { id },
                        Request.Scheme)!,
                    "self",
                    "DELETE")
            }
        };
    }
}