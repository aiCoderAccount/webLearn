namespace WebLearn.Models.ViewModels;

public class CourseDetailViewModel
{
    public Course Course { get; set; } = new();
    public List<UnitDetailViewModel> Units { get; set; } = new();
}

public class UnitDetailViewModel
{
    public Unit Unit { get; set; } = new();
    public List<Lesson> Lessons { get; set; } = new();
}
