using CourseraLens.Models;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;

namespace CourseraLens.gRPC;

public class GrpcService : Grpc.GrpcBase 
{
    private readonly ApplicationDbContext _context; 
    public GrpcService(ApplicationDbContext context)
    {
        _context = context; 
    }
    public override async Task<CourseResponse> GetCourse( 
    CourseRequest request,
        ServerCallContext scc)
    {
        var course = await _context.Courses 
            .Where(bg => bg.Id == request.Id)
            .FirstOrDefaultAsync();
        var response = new CourseResponse();
        if (course != null)
        {
            response.Id = course.Id;
            response.Title= course.Title;
            response.StudentsEnrolled = course.StudentsEnrolled;
        }
        return response; 
    }
}