using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CourseraLens.Models;

[Table("CertsTypes")]
public class CertType
{
    [Key] [Required] public int Id { get; set; }
    [Required] [MaxLength(50)] public string TypeName { get; set; } = null!;
    [Required] public DateTime CreatedDate { get; set; }
    [Required] public DateTime LastModifiedDate { get; set; }

    public ICollection<CoursesCertTypes>? CoursesCertTypes { get; set; }
}