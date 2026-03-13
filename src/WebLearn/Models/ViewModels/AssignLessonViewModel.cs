namespace WebLearn.Models.ViewModels;

public class AssignLessonViewModel
{
    public Unit Unit { get; set; } = new();
    public Course Course { get; set; } = new();
    public List<LessonAssignItem> AllLessons { get; set; } = new();
}

public class LessonAssignItem
{
    public int LessonId { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsAssigned { get; set; }
    public int SortOrder { get; set; }
}
