
using Dfe.PlanTech.Domain.Content.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dfe.PlanTech.Domain.Content.Models;

public class WarningComponentDbEntity : ContentComponentDbEntity, IWarningComponent<TextBodyDbEntity>, IHasRichText
{
    public string TextId { get; set; } = null!;
    public TextBodyDbEntity Text { get; set; } = null!;

    [NotMapped]
    [DontCopyValue]
    public RichTextContentDbEntity RichText
    {
        get => Text?.RichText;
        set
        {
            if (Text != null)
            {
                Text.RichText = value;
            }
        }
    }

    [NotMapped]
    [DontCopyValue]
    public long? RichTextId => Text?.RichTextId;

}