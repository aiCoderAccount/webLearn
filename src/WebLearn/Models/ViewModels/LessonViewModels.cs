using System.ComponentModel.DataAnnotations;

namespace WebLearn.Models.ViewModels;

public class LessonRenderViewModel
{
    public Lesson Lesson { get; set; } = new();
    public string RenderedHtml { get; set; } = string.Empty;
    public Course? Course { get; set; }
    public Unit? Unit { get; set; }
}

public class LessonEditViewModel
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    public string XmlContent { get; set; } = string.Empty;
}
