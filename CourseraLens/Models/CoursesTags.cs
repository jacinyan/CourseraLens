using System.ComponentModel.DataAnnotations;

namespace CourseraLens.Models;

public class CoursesTags
{
    [Key] [Required] public int CourseId { get; set; }
    [Key] [Required] public int TagId { get; set; }
    [Required] public DateTime CreatedDate { get; set; }

    public Course? Course { get; set; }
    public Tag? Tag { get; set; }
}