namespace Dfe.PlanTech.Domain.Content.Interfaces;

public interface IRichTextRenderer
{
    public string ToHtml(IRichTextContent content);
}