using CourseraLens.Models;
using Tag = CourseraLens.Models.Tag;

namespace CourseraLens.GraphQl;

public class Query
{
    [Serial]
    [UsePaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Course> GetCourses(
        [Service] ApplicationDbContext context)
    {
        return context.Courses;
    }

    [Serial]
    [UsePaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Tag> GetTags(
        [Service] ApplicationDbContext context)
    {
        return context.Tags;
    }
}