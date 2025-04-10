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
        int pageIndex = 0,
        int pageSize = 10
    )
    {
        var query = _context.Courses.Skip(pageIndex * pageSize).Take(pageSize);

        return new RestDto<Course[]>
        {
            Data = await query.ToArrayAsync(),
            PageIndex = pageIndex,
            PageSize = pageSize,
            TotalCount = await _context.Courses.CountAsync(),
            Links = new List<LinkDto>
            {
                new(
                    Url.Action(null, "Courses", new { pageIndex, pageSize },
                        Request.Scheme)!, "self", "GET")
            }
        };
    }
}