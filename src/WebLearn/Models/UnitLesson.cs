namespace WebLearn.Models;

public class UnitLesson
{
    public int Id { get; set; }
    public int UnitId { get; set; }
    public int LessonId { get; set; }
    public int SortOrder { get; set; }

    // Navigation
    public string LessonTitle { get; set; } = string.Empty;
}
