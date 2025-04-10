using CourseraLens.DTO;
using CourseraLens.Models;
using Microsoft.AspNetCore.Mvc;

namespace CourseraLens.Controllers;

[Route("[controller]")]
[ApiController]
public class CoursesController: ControllerBase
{   
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CoursesController> _logger;
    
    public CoursesController(ApplicationDbContext context,ILogger<CoursesController> logger)
    {   
        _context = context; 
        _logger = logger;
    }

    [HttpGet(Name = "GetCourses")]
    [ResponseCache(Location = ResponseCacheLocation.Any, Duration = 60)]
    public RestDTO<Course[]> Get()
    {
        return new RestDTO<Course[]>();
    }
}