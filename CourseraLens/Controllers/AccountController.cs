using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CourseraLens.DTO;
using CourseraLens.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace CourseraLens.Controllers;

[Route("[controller]/[action]")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CoursesController> _logger;
    private readonly SignInManager<ApiUser> _signInManager;
    private readonly UserManager<ApiUser> _userManager;

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
    public async Task<ActionResult> Register(RegisterDto input)
    {
        // Register a new user
        try
        {
            if (ModelState.IsValid)
            {
                var newUser = new ApiUser
                {
                    UserName = input.UserName,
                    Email = input.Email
                };
                if (input.Password != null)
                {
                    var result = await _userManager.CreateAsync(
                        newUser, input.Password);
                    if (!result.Succeeded)
                        throw new Exception(
                            $"Error: {string.Join(" ",
                                result.Errors.Select(e => e.Description))}");
                    _logger.LogInformation(
                        "User {userName} ({email}) has been created.",
                        newUser.UserName, newUser.Email);
                    return StatusCode(201,
                        $"User '{newUser.UserName}' has been created.");

                }
            }

            var details = new ValidationProblemDetails(ModelState)
                {
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                    Status = StatusCodes.Status400BadRequest
                };
            return new BadRequestObjectResult(details);
        }
        catch (Exception e)
        {
            var exceptionDetails = new ProblemDetails
            {
                Detail = e.Message,
                Status = StatusCodes.Status500InternalServerError,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
            };
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                exceptionDetails);
        }
    }

    [HttpPost]
    [ResponseCache(CacheProfileName = "NoCache")]
    public async Task<ActionResult> Login(LoginDTO input)
    {
        try
        {
            if (ModelState.IsValid)
            {
                if (input.UserName != null)
                {
                    var user = await _userManager.FindByNameAsync(input.UserName);
                    if (input.Password != null && (user == null
                                                   || !await _userManager.CheckPasswordAsync(
                                                       user, input.Password)))
                        throw new Exception("Invalid login attempt.");
                    var signingCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(
                                _configuration["JWT:SigningKey"] ?? string.Empty)),
                        SecurityAlgorithms.HmacSha256);

                    var claims = new List<Claim>();
                    if (user?.UserName != null)
                        claims.Add(new Claim(
                            ClaimTypes.Name, user.UserName));
                    if (user != null)
                        claims.AddRange(
                            (await _userManager.GetRolesAsync(user))
                            .Select(r => new Claim(ClaimTypes.Role, r)));

                    var jwtObject = new JwtSecurityToken(
                        _configuration["JWT:Issuer"],
                        _configuration["JWT:Audience"],
                        claims,
                        expires: DateTime.Now.AddSeconds(300),
                        signingCredentials: signingCredentials);

                    var jwtString = new JwtSecurityTokenHandler()
                        .WriteToken(jwtObject);

                    return StatusCode(
                        StatusCodes.Status200OK,
                        jwtString);
                }
            }

            var details = new ValidationProblemDetails(ModelState)
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Status = StatusCodes.Status400BadRequest
            };
            return new BadRequestObjectResult(details);
        }
        catch (Exception e)
        {
            var exceptionDetails = new ProblemDetails
            {
                Detail = e.Message,
                Status = StatusCodes.Status401Unauthorized,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
            };
            return StatusCode(
                StatusCodes.Status401Unauthorized,
                exceptionDetails);
        }
    }
}
