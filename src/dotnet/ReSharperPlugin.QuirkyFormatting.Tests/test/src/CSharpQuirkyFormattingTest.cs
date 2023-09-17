using JetBrains.ReSharper.FeaturesTestFramework.Formatter;
using JetBrains.ReSharper.Plugins.QuirkyFormatting.Psi.CodeStyle.Formatting;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.TestFramework;
using NUnit.Framework;

// ReSharper disable once CheckNamespace
namespace ReSharperPlugin.QuirkyFormatting.Tests;

[TestQuirks]
[Category("Formatting"), Category("CSharp")]
public class CSharpQuirkyFormattingTest : CodeFormatterWithExplicitSettingsTestBase<CSharpLanguage>
{
    protected override string RelativeTestDataPath => "CodeFormatter/CSharp";

    [TestSetting(
        typeof(QuirkyFormattingSettingsKey),
        nameof(QuirkyFormattingSettingsKey.ENFORCE_LINE_BREAKS_BETWEEN_STATEMENTS), 
        false
    )]
    [Test] public void TestStatementLinebreaks() { DoNamedTest2(); }

    [TestSetting(
        typeof(QuirkyFormattingSettingsKey), 
        nameof(QuirkyFormattingSettingsKey.ENFORCE_LINE_BREAKS_AFTER_LEFT_BRACES), 
        false
    )]
    [Test] public void TestLeftBraceLinebreaks() { DoNamedTest2(); }

    [TestSetting(
        typeof(QuirkyFormattingSettingsKey), 
        nameof(QuirkyFormattingSettingsKey.BANNER_STYLE_RIGHT_BRACE),
        true
    )]
    [Test] public void TestBannerStyleRightBrace() { DoNamedTest2(); }

    [TestSetting(
        typeof(QuirkyFormattingSettingsKey),
        nameof(QuirkyFormattingSettingsKey.ENFORCE_LOCAL_FUNCTION_DECLARATION_AND_INVOCATION_LINEBREAKS), 
        false
    )]
    [Test] public void TestLocalFunctionsLinebreaks() { DoNamedTest2(); }

    [TestSetting(
        typeof(QuirkyFormattingSettingsKey),
        nameof(QuirkyFormattingSettingsKey.INT_ALIGN_ATTRIBUTE_COMMAS), 
        true
    )]
    [Test] public void TestAlignAttributeInvocationCommas() { DoNamedTest2(); }
}