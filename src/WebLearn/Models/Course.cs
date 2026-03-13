namespace WebLearn.Models;

public class Course
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int InstructorId { get; set; }
    public int IsPublished { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
    public string UpdatedAt { get; set; } = string.Empty;

    // Navigation (populated by joins, not by Dapper automatically)
    public string InstructorName { get; set; } = string.Empty;
}
