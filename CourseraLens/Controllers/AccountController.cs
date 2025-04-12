using CourseraLens.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CourseraLens.Controllers;

[Route("[controller]/[action]")] 
[ApiController]
public class AccountController: ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CoursesController> _logger;
    private readonly IConfiguration _configuration;
    private readonly UserManager<ApiUser> _userManager; 
    private readonly SignInManager<ApiUser> _signInManager; 
    public AccountController(
        ApplicationDbContext context,
        ILogger<CoursesController> logger,
        IConfiguration configuration,
        UserManager<ApiUser> userManager, 
        SignInManager<ApiUser> signInManager) 
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
        _userManager = userManager; 
        _signInManager = signInManager; 
    }
    [HttpPost]
    [ResponseCache(CacheProfileName = "NoCache")]
    public async Task<ActionResult> Register() 
    {
        throw new NotImplementedException();
    }
    [HttpPost]
    [ResponseCache(CacheProfileName = "NoCache")]
    public async Task<ActionResult> Login() 
    {
        throw new NotImplementedException();
    } 
}