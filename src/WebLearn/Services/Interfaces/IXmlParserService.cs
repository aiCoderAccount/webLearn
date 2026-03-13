namespace WebLearn.Services.Interfaces;

public record ParseResult(string Html, IReadOnlyList<string> Errors, bool IsValid);

public interface IXmlParserService
{
    ParseResult ParseToHtml(string xml);
}
