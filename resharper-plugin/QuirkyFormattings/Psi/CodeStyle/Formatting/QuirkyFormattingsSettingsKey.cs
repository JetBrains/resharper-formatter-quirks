// ReSharper disable InconsistentNaming

using JetBrains.Application.Settings;
using JetBrains.ReSharper.Psi.CodeStyle;

namespace JetBrains.ReSharper.Plugins.QuirkyFormattings.Psi.CodeStyle.Formatting
{
  [SettingsKey(typeof(CodeFormattingSettingsKey), "Quirky formatter settings")]
  public class QuirkyFormattingsSettingsKey
  {
    [SettingsEntry(false, "Indent right brace banner-style")]
    public bool BANNER_STYLE_RIGHT_BRACE;

    [SettingsEntry(true, "Enforce line breaks after left braces")]
    public bool ENFORCE_LINE_BREAKS_AFTER_LEFT_BRACES;

    [SettingsEntry(true, "Enforce line breaks between statements")]
    public bool ENFORCE_LINE_BREAKS_BETWEEN_STATEMENTS;

    [SettingsEntry(true, "Enforce line breaks between a statement in which a local function is used and it's declaration")]
    public bool ENFORCE_LOCAL_FUNCTION_DECLARATION_AND_INVOCATION_LINEBREAKS;

    [SettingsEntry(false, "Align commas in attribute usages")]
    public bool INT_ALIGN_ATTRIBUTE_COMMAS;
  }
}