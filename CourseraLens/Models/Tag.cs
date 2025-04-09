using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CourseraLens.Models;

[Table("Tags")]
public class Tag
{
    [Key] [Required] public int Id { get; set; }
    [Required] [MaxLength(50)] public string TagName { get; set; } = null!;
    [Required] public DateTime CreatedDate { get; set; }
    [Required] public DateTime LastModifiedDate { get; set; }

    public ICollection<CoursesTags>? CoursesTags { get; set; }
}