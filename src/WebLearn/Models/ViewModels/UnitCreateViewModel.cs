using System.ComponentModel.DataAnnotations;

namespace WebLearn.Models.ViewModels;

public class UnitCreateViewModel
{
    public int Id { get; set; }
    public int CourseId { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    public int SortOrder { get; set; }
}
