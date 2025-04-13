namespace CourseraLens.Constants;

public class CustomLogEvents
{
    // CoursesController 
    public const int CoursesGet = 60110;
    public const int CoursesGetById = 60111;
    public const int CoursesPost = 60120;
    public const int CoursesPut = 60130;
    public const int CoursesDelete = 60140;

    // TagsController 
    public const int TagsGet = 60210;
    public const int TagsGetById = 60211;
    public const int TagsPost = 60220;
    public const int TagsPut = 60230;
    public const int TagsDelete = 60240;

    // Error
    public const int ErrorGet = 60001;
    
    // Other
    public const int DatabaseQuery = 60310;
    public const int AuthLogin = 60410;
    public const int CacheUpdate = 60510;
}