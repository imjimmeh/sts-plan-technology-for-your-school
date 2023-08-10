using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using System.Text;

namespace Dfe.PlanTech.Infrastructure.Contentful.Content.Renderers.Models.PartRenderers;

public class TableRenderer : BaseRichTextContentPartRender
{
    public TableRenderer() : base(RichTextNodeType.Table)
    {
    }

    public override StringBuilder AddHtml(IRichTextContent content, IRichTextContentPartRendererCollection rendererCollection, StringBuilder stringBuilder)
    {
        stringBuilder.Append("<table class=\"govuk-table\">");

        rendererCollection.RenderChildren(content, stringBuilder);

        stringBuilder.Append("</tbody>");
        stringBuilder.Append("</table>");

        return stringBuilder;
    }

}