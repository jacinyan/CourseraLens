using System.ComponentModel.DataAnnotations;

namespace CourseraLens.DTO;

public class CourseDto
{
    [Required]
    public int Id { get; set; }
    
    public string? Title { get; set; }      
    public int? StudentsEnrolled { get; set; }
}