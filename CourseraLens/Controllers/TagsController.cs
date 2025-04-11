using System.Linq.Dynamic.Core;
using CourseraLens.Attributes;
using CourseraLens.DTO;
using CourseraLens.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CourseraLens.Controllers;

[Route("[controller]")]
[ApiController]
public class TagsController: ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CoursesController> _logger;
    
    public TagsController(ApplicationDbContext context,
        ILogger<CoursesController> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    [HttpGet(Name = "GetTags")]
    [ResponseCache(Location = ResponseCacheLocation.Any, Duration = 60)]
    [ManualValidationFilter]
    public async Task<ActionResult<RestDto<Tag[]>>> Get(
        [FromQuery] RequestDto<TagDto> input
    )
    {   
        // Customize ModelState 
        if (!ModelState.IsValid) 
        {
            var details = new ValidationProblemDetails(ModelState);
            details.Extensions["traceId"] =
                System.Diagnostics.Activity.Current?.Id
                ?? HttpContext.TraceIdentifier;
            if (ModelState.Keys.Any(k => k == "PageSize"))
            {
                details.Type =
                    "https://tools.ietf.org/html/rfc7231#section-6.6.2";
                details.Status = StatusCodes.Status501NotImplemented;
                return new ObjectResult(details) {
                    StatusCode = StatusCodes.Status501NotImplemented
                };
            }
            else
            {
                details.Type =
                    "https://tools.ietf.org/html/rfc7231#section-6.5.1";
                details.Status = StatusCodes.Status400BadRequest;
                return new BadRequestObjectResult(details);
            }
        }
        
        var query = _context.Tags.AsQueryable();
        if (!string.IsNullOrEmpty(input.FilterQuery))
            query = query.Where(b => b.TagName.Contains(input.FilterQuery));
        var resultCount = await query.CountAsync();
        query = query
            .OrderBy($"{input.SortColumn} {input.SortOrder}")
            .Skip(input.PageIndex * input.PageSize)
            .Take(input.PageSize);

        return new RestDto<Tag[]>
        {
            Data = await query.ToArrayAsync(),
            PageIndex =input.PageIndex,
            PageSize =input.PageSize,
            ResultCount = resultCount,
            Links = new List<LinkDto>
            {
                new(
                    Url.Action(null, "Tags", new { input.PageIndex, input.PageSize },
                        Request.Scheme)!, "self", "GET")
            }
        };
    }
    
    [HttpPost(Name = "UpdateTag")]
    [ResponseCache(NoStore = true)]
    public async Task<RestDto<Tag?>> Post(TagDto model)
    {
        var tag = await _context.Tags
            .Where(b => b.Id == model.Id)
            .FirstOrDefaultAsync();
        if (tag != null)
        {
            if (!string.IsNullOrEmpty(model.TagName))
                tag.TagName = model.TagName;
            tag.LastModifiedDate = DateTime.Now;
            
            _context.Tags.Update(tag);  
            await _context.SaveChangesAsync();
        }
       
        return new RestDto<Tag?>
        {
            Data = tag,
            Links = new List<LinkDto>
            {
                new(
                    Url.Action(
                        null,
                        "Tags",
                        model,
                        Request.Scheme)!,
                    "self",
                    "POST")
            }
        };
    }
    
    [HttpDelete(Name = "DeleteTag")]
    [ResponseCache(NoStore = true)]
    public async Task<RestDto<Tag?>> Delete(int id)
    {
        var tag = await _context.Tags
            .Where(b => b.Id == id)
            .FirstOrDefaultAsync();
        if (tag != null)
        {
            _context.Tags.Remove(tag);
            await _context.SaveChangesAsync();
        }
        
        return new RestDto<Tag?>
        {
            Data = tag,
            Links = new List<LinkDto>
            {
                new(
                    Url.Action(
                        null,
                        "Tags",
                        new { id },
                        Request.Scheme)!,
                    "self",
                    "DELETE")
            }
        };
    }
}