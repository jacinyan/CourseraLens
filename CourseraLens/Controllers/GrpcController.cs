using CourseraLens.gRPC;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Mvc;

namespace CourseraLens.Controllers;

[Route("[controller]/[action]")]
[ApiController]
public class GrpcController : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<CourseResponse> GetCourse(int id)
    {
        using var channel = GrpcChannel
            .ForAddress("https://localhost:40443"); 
        var client = new gRPC.Grpc.GrpcClient(channel); 
        var response = await client.GetCourseAsync( 
        new CourseRequest { Id = id });
        return response; 
    }
}