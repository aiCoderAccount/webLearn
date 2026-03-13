using WebLearn.Models;
using WebLearn.Models.ViewModels;
using WebLearn.Repositories.Interfaces;
using WebLearn.Services.Interfaces;

namespace WebLearn.Services;

public class LessonService : ILessonService
{
    private readonly ILessonRepository _lessonRepo;
    private readonly IXmlParserService _parser;

    public LessonService(ILessonRepository lessonRepo, IXmlParserService parser)
    {
        _lessonRepo = lessonRepo;
        _parser = parser;
    }

    public async Task<Lesson?> GetByIdAsync(int id) =>
        await _lessonRepo.GetByIdAsync(id);

    public async Task<IEnumerable<Lesson>> GetByInstructorIdAsync(int instructorId) =>
        await _lessonRepo.GetByInstructorIdAsync(instructorId);

    public async Task<LessonRenderViewModel?> RenderLessonAsync(int id)
    {
        var lesson = await _lessonRepo.GetByIdAsync(id);
        if (lesson == null) return null;

        var result = _parser.ParseToHtml(lesson.XmlContent);
        return new LessonRenderViewModel
        {
            Lesson = lesson,
            RenderedHtml = result.IsValid ? result.Html : $"<div class=\"alert alert-danger\">Lesson XML is invalid: {string.Join(", ", result.Errors)}</div>"
        };
    }

    public string PreviewXml(string xml)
    {
        var result = _parser.ParseToHtml(xml);
        if (!result.IsValid)
            return $"<div class=\"alert alert-danger\"><strong>Parse errors:</strong><ul>{string.Concat(result.Errors.Select(e => $"<li>{System.Web.HttpUtility.HtmlEncode(e)}</li>"))}</ul></div>";
        return result.Html;
    }

    public async Task<(bool Success, string? Error, int Id)> CreateAsync(LessonEditViewModel vm, int instructorId)
    {
        var parseResult = _parser.ParseToHtml(vm.XmlContent);
        if (!parseResult.IsValid)
            return (false, "XML is invalid: " + string.Join("; ", parseResult.Errors), 0);

        var now = DateTime.UtcNow.ToString("o");
        var lesson = new Lesson
        {
            Title = vm.Title,
            XmlContent = vm.XmlContent,
            InstructorId = instructorId,
            CreatedAt = now,
            UpdatedAt = now
        };

        var id = await _lessonRepo.CreateAsync(lesson);
        return (true, null, id);
    }

    public async Task<(bool Success, string? Error)> UpdateAsync(LessonEditViewModel vm, int instructorId)
    {
        var existing = await _lessonRepo.GetByIdAsync(vm.Id);
        if (existing == null) return (false, "Lesson not found.");
        if (existing.InstructorId != instructorId) return (false, "Access denied.");

        var parseResult = _parser.ParseToHtml(vm.XmlContent);
        if (!parseResult.IsValid)
            return (false, "XML is invalid: " + string.Join("; ", parseResult.Errors));

        var updated = new Lesson
        {
            Id = existing.Id,
            Title = vm.Title,
            XmlContent = vm.XmlContent,
            InstructorId = existing.InstructorId,
            CreatedAt = existing.CreatedAt,
            UpdatedAt = DateTime.UtcNow.ToString("o")
        };

        await _lessonRepo.UpdateAsync(updated);
        return (true, null);
    }

    public async Task<bool> DeleteAsync(int id, int instructorId)
    {
        var lesson = await _lessonRepo.GetByIdAsync(id);
        if (lesson == null || lesson.InstructorId != instructorId) return false;
        await _lessonRepo.DeleteAsync(id);
        return true;
    }
}
