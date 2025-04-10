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
    public async Task<RestDto<Course[]>> Get()
    {
        var query = _context.Courses;

        return new RestDto<Course[]>
        {
            Data = await query.ToArrayAsync(),
            Links = new List<LinkDto>
            {
               new LinkDto( Url.Action(null,"Courses", null, Request.Scheme)!, "self", "GET"),
            }
        };
    }
}