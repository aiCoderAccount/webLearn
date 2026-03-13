using FluentAssertions;
using WebLearn.Services;
using Xunit;

namespace WebLearn.Tests.Services;

public class XmlParserServiceTests
{
    private readonly XmlParserService _sut = new();

    [Fact]
    public void ParseToHtml_ValidLesson_ReturnsHtml()
    {
        var xml = "<lesson title=\"Test\" level=\"beginner\"><objective>Learn stuff</objective><summary>Done.</summary></lesson>";
        var result = _sut.ParseToHtml(xml);
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
        result.Html.Should().Contain("Test");
        result.Html.Should().Contain("Learn stuff");
    }

    [Fact]
    public void ParseToHtml_EmptyXml_ReturnsInvalid()
    {
        var result = _sut.ParseToHtml("");
        result.IsValid.Should().BeFalse();
        result.Html.Should().BeEmpty();
    }

    [Fact]
    public void ParseToHtml_WrongRoot_ReturnsError()
    {
        var result = _sut.ParseToHtml("<course>bad</course>");
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainMatch("*lesson*");
    }

    [Fact]
    public void ParseToHtml_MalformedXml_ReturnsError()
    {
        var result = _sut.ParseToHtml("<lesson>unclosed");
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData("beginner", "bg-success")]
    [InlineData("intermediate", "bg-warning")]
    [InlineData("advanced", "bg-danger")]
    public void ParseToHtml_LevelBadge_RendersCorrectClass(string level, string expectedClass)
    {
        var xml = $"<lesson title=\"T\" level=\"{level}\"><summary>X</summary></lesson>";
        var result = _sut.ParseToHtml(xml);
        result.IsValid.Should().BeTrue();
        result.Html.Should().Contain(expectedClass);
    }

    [Fact]
    public void ParseToHtml_XssInContent_IsEncoded()
    {
        var xml = "<lesson title=\"&lt;script&gt;alert(1)&lt;/script&gt;\" level=\"beginner\"><summary>test</summary></lesson>";
        var result = _sut.ParseToHtml(xml);
        result.Html.Should().NotContain("<script>alert(1)</script>");
    }

    [Fact]
    public void ParseToHtml_DtdInjection_IsProhibited()
    {
        var xml = "<?xml version=\"1.0\"?><!DOCTYPE test [<!ENTITY xxe SYSTEM \"file:///etc/passwd\">]><lesson title=\"t\" level=\"beginner\"><summary>&xxe;</summary></lesson>";
        var result = _sut.ParseToHtml(xml);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void ParseToHtml_AllTwelveTags_Parsed()
    {
        var xml = @"<lesson title=""Full"" level=""advanced"">
  <objective>Obj1</objective>
  <example title=""Ex"">Sample</example>
  <quiz title=""Q""><question type=""multiple-choice"" text=""Q1?""><answer correct=""true"">Yes</answer></question></quiz>
  <note type=""info"">Note text</note>
  <definition term=""Term"">Definition body</definition>
  <diagram alt=""D"" caption=""Cap"">ASCII art</diagram>
  <exercise difficulty=""easy"">Do this</exercise>
  <codeblock language=""python"">print('hi')</codeblock>
  <summary><item>Point 1</item></summary>
</lesson>";
        var result = _sut.ParseToHtml(xml);
        result.IsValid.Should().BeTrue();
        result.Html.Should().Contain("Obj1");
        result.Html.Should().Contain("Ex");
        result.Html.Should().Contain("Note text");
        result.Html.Should().Contain("Term");
        result.Html.Should().Contain("ASCII art");
        result.Html.Should().Contain("language-python");
        result.Html.Should().Contain("Point 1");
    }
}
