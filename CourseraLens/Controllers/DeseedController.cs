using CourseraLens.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CourseraLens.Controllers;

[Authorize]
[Route("[controller]")]
[ApiController]
public class DeseedController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DeseedController> _logger;

    public DeseedController(
        ApplicationDbContext context,
        ILogger<DeseedController> logger)
    {
        _context = context;
        _logger = logger;
    }
    

    [HttpDelete]
    public async Task<IActionResult> DeseedDatabase()
    {
        try
        {
            // Delete all records in proper order (child tables first)
            await _context.Database.ExecuteSqlRawAsync(
                "DELETE FROM CoursesTags");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM Tags");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM Courses");

            await _context.Database.ExecuteSqlRawAsync(
                "DBCC CHECKIDENT ('Courses', RESEED, 0)");
            await _context.Database.ExecuteSqlRawAsync(
                "DBCC CHECKIDENT ('Tags', RESEED, 0)");

            _logger.LogInformation("Database successfully deseeded");
            return Ok(new { Message = "Database deseeding completed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during database deseeding");
            return StatusCode(500,
                new { Error = "Deseeding failed", Details = ex.Message });
        }
    }
}