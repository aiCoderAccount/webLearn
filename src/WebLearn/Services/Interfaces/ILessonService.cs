using WebLearn.Models;
using WebLearn.Models.ViewModels;

namespace WebLearn.Services.Interfaces;

public interface ILessonService
{
    Task<Lesson?> GetByIdAsync(int id);
    Task<IEnumerable<Lesson>> GetByInstructorIdAsync(int instructorId);
    Task<LessonRenderViewModel?> RenderLessonAsync(int id);
    Task<(bool Success, string? Error, int Id)> CreateAsync(LessonEditViewModel vm, int instructorId);
    Task<(bool Success, string? Error)> UpdateAsync(LessonEditViewModel vm, int instructorId);
    Task<bool> DeleteAsync(int id, int instructorId);
    string PreviewXml(string xml);
}
