using System.Diagnostics;
using System.Linq.Dynamic.Core;
using System.Text.Json;
using CourseraLens.Attributes;
using CourseraLens.Constants;
using CourseraLens.DTO;
using CourseraLens.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Tag = CourseraLens.Models.Tag;

namespace CourseraLens.Controllers;

[Route("[controller]")]
[ApiController]
public class TagsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<TagsController> _logger;
    private readonly IMemoryCache _memoryCache;

    public TagsController(
        ApplicationDbContext context,
        ILogger<TagsController> logger,
        IMemoryCache memoryCache)
    {
        _context = context;
        _logger = logger;
        _memoryCache = memoryCache;
    }

    [HttpGet(Name = "GetTags")]
    [ResponseCache(CacheProfileName = "Any-60")]
    [ManualValidationFilter]
    public async Task<ActionResult<RestDto<Tag[]>>> Get(
        [FromQuery] RequestDto<TagDto> input)
    {
        if (!ModelState.IsValid)
        {
            var details = new ValidationProblemDetails(ModelState);
            details.Extensions["traceId"] =
                Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            if (ModelState.Keys.Any(k => k == "PageSize"))
            {
                details.Type =
                    "https://tools.ietf.org/html/rfc7231#section-6.6.2";
                details.Status = StatusCodes.Status501NotImplemented;
                return new ObjectResult(details)
                    { StatusCode = StatusCodes.Status501NotImplemented };
            }

            details.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
            details.Status = StatusCodes.Status400BadRequest;
            return new BadRequestObjectResult(details);
        }

        var query = _context.Tags.AsQueryable();
        if (!string.IsNullOrEmpty(input.FilterQuery))
            query = query.Where(b => b.TagName.Contains(input.FilterQuery));
        var resultCount = await query.CountAsync();

        var cacheKey = $"{input.GetType().Name}-{JsonSerializer.Serialize(new
        {
            input.PageIndex,
            input.PageSize,
            input.SortColumn,
            input.SortOrder,
            input.FilterQuery
        }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })}";
        if (_memoryCache.TryGetValue(cacheKey, out Tag[]? result))
            return new RestDto<Tag[]?>
            {
                Data = result,
                PageIndex = input.PageIndex,
                PageSize = input.PageSize,
                ResultCount = resultCount,
                Links = new List<LinkDto>
                {
                    new(
                        Url.Action(null, "Tags",
                            new { input.PageIndex, input.PageSize },
                            Request.Scheme)!,
                        "self",
                        "GET")
                }
            };
        query = query
            .OrderBy($"{input.SortColumn} {input.SortOrder}")
            .Skip(input.PageIndex * input.PageSize)
            .Take(input.PageSize);

        result = await query.ToArrayAsync();
        _memoryCache.Set(cacheKey, result, new TimeSpan(0, 0, 30));

        return new RestDto<Tag[]?>
        {
            Data = result,
            PageIndex = input.PageIndex,
            PageSize = input.PageSize,
            ResultCount = resultCount,
            Links = new List<LinkDto>
            {
                new(
                    Url.Action(null, "Tags",
                        new { input.PageIndex, input.PageSize },
                        Request.Scheme)!,
                    "self",
                    "GET")
            }
        };
    }

    [Authorize(Roles = RoleNames.Curator)]
    [HttpPost(Name = "UpdateTag")]
    [ResponseCache(CacheProfileName = "NoCache")]
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

    [Authorize(Roles = RoleNames.Admin)]
    [HttpDelete(Name = "DeleteTag")]
    [ResponseCache(CacheProfileName = "NoCache")]
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