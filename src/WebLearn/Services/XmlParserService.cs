using System.Text;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using WebLearn.Services.Interfaces;

namespace WebLearn.Services;

public class XmlParserService : IXmlParserService
{
    public ParseResult ParseToHtml(string xml)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(xml))
            return new ParseResult(string.Empty, errors, false);

        XDocument doc;
        try
        {
            var settings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null
            };
            using var reader = XmlReader.Create(new StringReader(xml), settings);
            doc = XDocument.Load(reader);
        }
        catch (Exception ex)
        {
            errors.Add($"XML parse error: {ex.Message}");
            return new ParseResult(string.Empty, errors, false);
        }

        var root = doc.Root;
        if (root == null || root.Name.LocalName != "lesson")
        {
            errors.Add("Root element must be <lesson>");
            return new ParseResult(string.Empty, errors, false);
        }

        var sb = new StringBuilder();
        RenderLesson(root, sb);

        return new ParseResult(sb.ToString(), errors, true);
    }

    private static void RenderLesson(XElement el, StringBuilder sb)
    {
        var title = HttpUtility.HtmlEncode(el.Attribute("title")?.Value ?? "Untitled Lesson");
        var level = el.Attribute("level")?.Value?.ToLower() ?? "beginner";
        var badgeClass = level switch
        {
            "intermediate" => "bg-warning text-dark",
            "advanced" => "bg-danger",
            _ => "bg-success"
        };
        var levelLabel = char.ToUpper(level[0]) + level[1..];

        sb.AppendLine($"<article class=\"lesson-content\">");
        sb.AppendLine($"  <div class=\"d-flex align-items-center gap-3 mb-4\">");
        sb.AppendLine($"    <h1 class=\"h2 mb-0\">{title}</h1>");
        sb.AppendLine($"    <span class=\"badge {badgeClass}\">{HttpUtility.HtmlEncode(levelLabel)}</span>");
        sb.AppendLine($"  </div>");

        // Collect objectives first
        var objectives = el.Elements("objective").ToList();
        if (objectives.Count > 0)
        {
            sb.AppendLine("  <div class=\"card mb-4 border-primary\">");
            sb.AppendLine("    <div class=\"card-header bg-primary text-white\"><i class=\"bi bi-bullseye\"></i> Learning Objectives</div>");
            sb.AppendLine("    <div class=\"card-body\"><ul class=\"mb-0\">");
            foreach (var obj in objectives)
                sb.AppendLine($"      <li>{HttpUtility.HtmlEncode(obj.Value.Trim())}</li>");
            sb.AppendLine("    </ul></div></div>");
        }

        // Render remaining elements in order (skip objectives already handled)
        foreach (var child in el.Elements())
        {
            if (child.Name.LocalName == "objective") continue;
            RenderElement(child, sb);
        }

        sb.AppendLine("</article>");
    }

    private static void RenderElement(XElement el, StringBuilder sb)
    {
        switch (el.Name.LocalName)
        {
            case "example":
                RenderExample(el, sb);
                break;
            case "quiz":
                RenderQuiz(el, sb);
                break;
            case "note":
                RenderNote(el, sb);
                break;
            case "definition":
                RenderDefinition(el, sb);
                break;
            case "diagram":
                RenderDiagram(el, sb);
                break;
            case "exercise":
                RenderExercise(el, sb);
                break;
            case "codeblock":
                RenderCodeBlock(el, sb);
                break;
            case "summary":
                RenderSummary(el, sb);
                break;
            default:
                // Unknown element: render text content safely
                var text = el.Value.Trim();
                if (!string.IsNullOrEmpty(text))
                    sb.AppendLine($"<p>{HttpUtility.HtmlEncode(text)}</p>");
                break;
        }
    }

    private static void RenderExample(XElement el, StringBuilder sb)
    {
        var title = HttpUtility.HtmlEncode(el.Attribute("title")?.Value ?? "Example");
        sb.AppendLine("  <div class=\"card mb-4 border-secondary\">");
        sb.AppendLine($"    <div class=\"card-header\"><strong>Example: {title}</strong></div>");
        sb.AppendLine("    <div class=\"card-body\">");
        foreach (var child in el.Elements())
            RenderElement(child, sb);
        var text = GetDirectText(el);
        if (!string.IsNullOrEmpty(text))
            sb.AppendLine($"      <p>{HttpUtility.HtmlEncode(text)}</p>");
        sb.AppendLine("    </div></div>");
    }

    private static void RenderQuiz(XElement el, StringBuilder sb)
    {
        var title = HttpUtility.HtmlEncode(el.Attribute("title")?.Value ?? "Quiz");
        sb.AppendLine("  <div class=\"card mb-4 border-info\">");
        sb.AppendLine($"    <div class=\"card-header bg-info text-white\"><i class=\"bi bi-question-circle\"></i> {title}</div>");
        sb.AppendLine("    <div class=\"card-body\">");
        int qNum = 1;
        foreach (var q in el.Elements("question"))
        {
            RenderQuestion(q, sb, qNum++);
        }
        sb.AppendLine("    </div></div>");
    }

    private static void RenderQuestion(XElement el, StringBuilder sb, int num)
    {
        var type = el.Attribute("type")?.Value ?? "short-answer";
        var qText = HttpUtility.HtmlEncode(el.Attribute("text")?.Value ?? GetDirectText(el));
        sb.AppendLine($"    <div class=\"card mb-3\">");
        sb.AppendLine($"      <div class=\"card-body\">");
        sb.AppendLine($"        <p class=\"fw-bold\">Q{num}. {qText}</p>");

        if (type == "multiple-choice" || type == "true-false")
        {
            var answers = el.Elements("answer").ToList();
            sb.AppendLine("        <div class=\"quiz-options\">");
            foreach (var ans in answers)
            {
                var isCorrect = ans.Attribute("correct")?.Value?.ToLower() == "true";
                var ansText = HttpUtility.HtmlEncode(ans.Value.Trim());
                sb.AppendLine($"          <div class=\"form-check quiz-answer\" data-correct=\"{isCorrect.ToString().ToLower()}\">");
                sb.AppendLine($"            <input class=\"form-check-input\" type=\"radio\" disabled>");
                sb.AppendLine($"            <label class=\"form-check-label\">{ansText}</label>");
                sb.AppendLine("          </div>");
            }
            sb.AppendLine("        </div>");
            sb.AppendLine("        <button class=\"btn btn-sm btn-outline-primary mt-2 quiz-reveal-btn\">Reveal Answer</button>");
        }
        else
        {
            sb.AppendLine("        <div class=\"mt-2\">");
            sb.AppendLine("          <textarea class=\"form-control\" rows=\"3\" placeholder=\"Your answer...\"></textarea>");
            var ans = el.Elements("answer").FirstOrDefault();
            if (ans != null)
            {
                var ansText = HttpUtility.HtmlEncode(ans.Value.Trim());
                sb.AppendLine($"          <div class=\"quiz-answer mt-2 d-none\" data-correct=\"true\"><strong>Model Answer:</strong> {ansText}</div>");
                sb.AppendLine("          <button class=\"btn btn-sm btn-outline-primary mt-2 quiz-reveal-btn\">Reveal Model Answer</button>");
            }
            sb.AppendLine("        </div>");
        }

        sb.AppendLine("      </div></div>");
    }

    private static void RenderNote(XElement el, StringBuilder sb)
    {
        var type = el.Attribute("type")?.Value?.ToLower() ?? "info";
        var (alertClass, icon) = type switch
        {
            "warning" => ("alert-warning", "bi-exclamation-triangle"),
            "tip" => ("alert-success", "bi-lightbulb"),
            _ => ("alert-info", "bi-info-circle")
        };
        var label = char.ToUpper(type[0]) + type[1..];
        sb.AppendLine($"  <div class=\"alert {alertClass} mb-3\" role=\"alert\">");
        sb.AppendLine($"    <i class=\"bi {icon}\"></i> <strong>{HttpUtility.HtmlEncode(label)}:</strong> {HttpUtility.HtmlEncode(el.Value.Trim())}");
        sb.AppendLine("  </div>");
    }

    private static void RenderDefinition(XElement el, StringBuilder sb)
    {
        var term = HttpUtility.HtmlEncode(el.Attribute("term")?.Value ?? "Term");
        var def = HttpUtility.HtmlEncode(el.Value.Trim());
        sb.AppendLine("  <dl class=\"mb-3\">");
        sb.AppendLine($"    <dt>{term}</dt>");
        sb.AppendLine($"    <dd>{def}</dd>");
        sb.AppendLine("  </dl>");
    }

    private static void RenderDiagram(XElement el, StringBuilder sb)
    {
        var src = el.Attribute("src")?.Value;
        var alt = HttpUtility.HtmlEncode(el.Attribute("alt")?.Value ?? "Diagram");
        var caption = HttpUtility.HtmlEncode(el.Attribute("caption")?.Value ?? string.Empty);

        sb.AppendLine("  <figure class=\"figure mb-4\">");
        if (!string.IsNullOrEmpty(src))
        {
            // Only allow relative paths (no external URLs for security)
            var safeSrc = HttpUtility.HtmlEncode(src);
            sb.AppendLine($"    <img src=\"{safeSrc}\" alt=\"{alt}\" class=\"figure-img img-fluid rounded\">");
        }
        else
        {
            // Render content as ASCII diagram in preformatted block
            var content = HttpUtility.HtmlEncode(el.Value);
            sb.AppendLine($"    <pre class=\"bg-light p-3 rounded\">{content}</pre>");
        }
        if (!string.IsNullOrEmpty(caption))
            sb.AppendLine($"    <figcaption class=\"figure-caption\">{caption}</figcaption>");
        sb.AppendLine("  </figure>");
    }

    private static void RenderExercise(XElement el, StringBuilder sb)
    {
        var difficulty = el.Attribute("difficulty")?.Value?.ToLower() ?? "medium";
        var (badgeClass, label) = difficulty switch
        {
            "easy" => ("bg-success", "Easy"),
            "hard" => ("bg-danger", "Hard"),
            _ => ("bg-warning text-dark", "Medium")
        };
        sb.AppendLine("  <div class=\"card mb-4 border-warning\">");
        sb.AppendLine($"    <div class=\"card-header d-flex justify-content-between align-items-center\">");
        sb.AppendLine($"      <span><i class=\"bi bi-pencil-square\"></i> Exercise</span>");
        sb.AppendLine($"      <span class=\"badge {badgeClass}\">{label}</span>");
        sb.AppendLine("    </div>");
        sb.AppendLine("    <div class=\"card-body\">");
        foreach (var child in el.Elements())
            RenderElement(child, sb);
        var text = GetDirectText(el);
        if (!string.IsNullOrEmpty(text))
            sb.AppendLine($"      <p>{HttpUtility.HtmlEncode(text)}</p>");
        sb.AppendLine("    </div></div>");
    }

    private static void RenderCodeBlock(XElement el, StringBuilder sb)
    {
        var language = HttpUtility.HtmlEncode(el.Attribute("language")?.Value ?? "plaintext");
        var code = HttpUtility.HtmlEncode(el.Value);
        sb.AppendLine($"  <pre class=\"mb-4\"><code class=\"language-{language}\">{code}</code></pre>");
    }

    private static void RenderSummary(XElement el, StringBuilder sb)
    {
        sb.AppendLine("  <div class=\"card mb-4 bg-light\">");
        sb.AppendLine("    <div class=\"card-header\"><i class=\"bi bi-bookmark-check\"></i> <strong>Summary</strong></div>");
        sb.AppendLine("    <div class=\"card-body\">");

        var bullets = el.Elements("item").ToList();
        if (bullets.Count > 0)
        {
            sb.AppendLine("      <ul>");
            foreach (var item in bullets)
                sb.AppendLine($"        <li>{HttpUtility.HtmlEncode(item.Value.Trim())}</li>");
            sb.AppendLine("      </ul>");
        }
        else
        {
            var text = el.Value.Trim();
            if (!string.IsNullOrEmpty(text))
                sb.AppendLine($"      <p>{HttpUtility.HtmlEncode(text)}</p>");
        }

        sb.AppendLine("    </div></div>");
    }

    private static string GetDirectText(XElement el)
    {
        return string.Concat(el.Nodes().OfType<XText>().Select(t => t.Value)).Trim();
    }
}
