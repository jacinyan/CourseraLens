namespace CourseraLens.DTO;

public class RestDto<T>
{
    public T Data { get; set; } = default!;
    public int? PageIndex { get; set; }
    public int? PageSize { get; set; }
    public int? ResultCount { get; set; }
    public List<LinkDto> Links { get; set; } = new();
}