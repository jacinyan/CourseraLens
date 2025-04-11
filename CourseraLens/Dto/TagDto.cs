using System.ComponentModel.DataAnnotations;

namespace CourseraLens.DTO;

public class TagDto
{
    [Required] public int Id { get; set; }

    public string? TagName { get; set; }
}