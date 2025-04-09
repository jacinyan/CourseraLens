using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CourseraLens.Models;

[Table("Organizations")]
public class Organization
{
    [Key] [Required] public int Id { get; set; }
    [Required] [MaxLength(100)] public string Name { get; set; } = null!;
    [Required] public DateTime CreatedDate { get; set; }
    [Required] public DateTime LastModifiedDate { get; set; }

    public ICollection<CoursesOrganizations>? CoursesOrganizations { get; set; }

}