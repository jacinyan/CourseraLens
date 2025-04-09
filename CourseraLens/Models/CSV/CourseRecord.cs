using CsvHelper.Configuration.Attributes;

namespace CourseraLens.Models.CSV;

public class CourseRecord
{
    [Name("id")] public int? Id { get; set; }

    [Name("course_title")] public string? Title { get; set; }

    [Name("course_organization")] public string? Organization { get; set; }

    [Name("course_certification_type")] public string? CertType { get; set; }

    [Name("course_rating")] public decimal? Rating { get; set; }

    [Name("course_difficulty")] public string? Difficulty { get; set; }

    [Name("course_students_enrolled")]
    public int? StudentsEnrolled { get; set; }

    [Name("course_tags")] public string? Tags { get; set; }
}