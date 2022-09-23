using JetBrains.Annotations;
using JetBrains.Application.Components;
using JetBrains.Application.Settings;
using JetBrains.Lifetimes;
using JetBrains.ReSharper.Feature.Services.OptionPages;
using JetBrains.ReSharper.Feature.Services.OptionPages.CodeStyle;
using JetBrains.ReSharper.Features.Altering.CodeFormatter.CSharp;

namespace JetBrains.ReSharper.Plugins.FormatterQuirks.Psi.CodeStyle.Formatting
{
  [FormattingSettingsPresentationComponent]
  public class QuirkyFormattingSettingsSchema : OthersPageSchemaPart
  {
    public QuirkyFormattingSettingsSchema(
      Lifetime lifetime,
      [NotNull] IContextBoundSettingsStoreLive smartContext,
      [NotNull] IValueEditorViewModelFactory itemViewModelFactory,
      [NotNull] IComponentContainer container,
      [NotNull] ISettingsToHide settingsToHide
    ) : base(lifetime, smartContext, itemViewModelFactory, container, settingsToHide)
    {
    }

    protected override string PagePartName => "Quirky code style";

    protected override void DescribePart(SchemaBuilder builder)
    {
      // The formatter runs over the code that is given to the setting inside ItemFor.
      // It is used to demo the setting's effect.
      
      builder
        .ItemFor((QuirkyFormattingSettingsKey x) => x.ENFORCE_LINE_BREAKS_BETWEEN_STATEMENTS,
          "public static void Main(){Statement1(); Statement2();}")
        .ItemFor((QuirkyFormattingSettingsKey x) => x.ENFORCE_LINE_BREAKS_AFTER_LEFT_BRACES,
          "public static void Main(){Statement();}")
        .ItemFor((QuirkyFormattingSettingsKey x) => x.ENFORCE_LOCAL_FUNCTION_DECLARATION_AND_INVOCATION_LINEBREAKS,
          "public static void Main(){LocalF(); void LocalF(){};}")
        .ItemFor((QuirkyFormattingSettingsKey x) => x.INT_ALIGN_ATTRIBUTE_COMMAS,
          "[Attr(\r\nLittleString=\"smol\",\r\nLongString=\"This is some very long string literal\",\r\nLittleString1=\"smolagain\"\r\n)]\r\npublic static void Main(){}")
        .ItemFor((QuirkyFormattingSettingsKey x) => x.BANNER_STYLE_RIGHT_BRACE,
          "public static void Main(){Console.WriteLine();}");

      // Don't call Build() here, it will complain.
      // Build() is called in the parent schema's Describe() method.
    }
  }
}