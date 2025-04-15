using CourseraLens.Constants;
using CourseraLens.DTO;
using CourseraLens.Models;
using HotChocolate.Authorization;
using Microsoft.EntityFrameworkCore;
using Tag = CourseraLens.Models.Tag;

namespace CourseraLens.GraphQl;

public class Mutation
{
    [Serial]
    [Authorize(Roles = new[] { RoleNames.Curator })]
    public async Task<Course?> UpdateCourse(
        [Service] ApplicationDbContext context, CourseDto model)
    {
        var course = await context.Courses
            .Where(b => b.Id == model.Id)
            .FirstOrDefaultAsync();
        if (course != null)
        {
            if (!string.IsNullOrEmpty(model.Title))
                course.Title = model.Title;
            if (model.StudentsEnrolled is > 0)
                course.StudentsEnrolled = model.StudentsEnrolled.Value;
            course.LastModifiedDate = DateTime.Now;
            context.Courses.Update(course);
            await context.SaveChangesAsync();
        }

        return course;
    }

    [Serial]
    [Authorize(Roles = new[] { RoleNames.Admin })]
    public async Task DeleteCourse(
        [Service] ApplicationDbContext context, int id)
    {
        var course = await context.Courses
            .Where(b => b.Id == id)
            .FirstOrDefaultAsync();
        if (course != null)
        {
            context.Courses.Remove(course);
            await context.SaveChangesAsync();
        }
    }

    public async Task<Tag?> UpdateTag(
        [Service] ApplicationDbContext context,
        [Service] ILogger<Mutation> logger,
        TagDto model)
    {
        var tag = await context.Tags
            .Where(t => t.Id == model.Id)
            .FirstOrDefaultAsync();

        if (tag != null)
        {
            if (!string.IsNullOrEmpty(model.TagName))
                tag.TagName = model.TagName;

            context.Tags.Update(tag);
            await context.SaveChangesAsync();
        }

        return tag;
    }

    [Serial]
    [Authorize(Roles = new[] { RoleNames.Admin })]
    public async Task DeleteTag(
        [Service] ApplicationDbContext context,
        int id)
    {
        var tag = await context.Tags
            .Where(t => t.Id == id)
            .FirstOrDefaultAsync();
        if (tag != null)
        {
            context.Tags.Remove(tag);
            await context.SaveChangesAsync();
        }
    }
}