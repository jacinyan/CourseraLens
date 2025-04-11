using System.Linq.Dynamic.Core;
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
    public async Task<RestDto<Tag[]>> Get(
        [FromQuery] RequestDto<TagDto> input
    )
    {
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