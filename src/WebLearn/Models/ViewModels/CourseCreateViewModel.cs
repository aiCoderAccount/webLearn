using System.ComponentModel.DataAnnotations;

namespace WebLearn.Models.ViewModels;

public class CourseCreateViewModel
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    public bool IsPublished { get; set; }
}
