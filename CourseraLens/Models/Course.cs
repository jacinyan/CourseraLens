using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CourseraLens.Models;

[Table("Courses")]
public class Course
{
    [Key] [Required] public int Id { get; set; }
    [Required] [MaxLength(255)] public string Title { get; set; } = null!;
    [Required] [MaxLength(100)] public string Organization { get; set; } = null!;
    [Required] [MaxLength(50)] public string CertType { get; set; } = null!;
    [Required] [Precision(4, 2)] public decimal Rating { get; set; }
    [Required] [MaxLength(20)] public string Difficulty{ get; set; } = null!;
    [Required] public int StudentsEnrolled{ get; set; }
    [Required] public DateTime CreatedDate { get; set; }
    [Required] public DateTime LastModifiedDate { get; set; }


    public ICollection<CoursesOrganizations>? CoursesOrganizations { get; set; }
    public ICollection<CoursesCertTypes>? CoursesCertTypes { get; set; }
}