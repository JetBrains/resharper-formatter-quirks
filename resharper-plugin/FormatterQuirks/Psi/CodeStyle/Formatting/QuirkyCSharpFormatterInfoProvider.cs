using System.Collections.Generic;
using System.Linq;
using JetBrains.Application.Settings;
using JetBrains.Application.Settings.Calculated.Interface;
using JetBrains.Application.Threading;
using JetBrains.Lifetimes;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Impl.CodeStyle.Formatter;
using JetBrains.ReSharper.Psi.CSharp.Impl.Tree;
using JetBrains.ReSharper.Psi.CSharp.Parsing;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Impl.CodeStyle;
using JetBrains.ReSharper.Psi.Tree;

namespace JetBrains.ReSharper.Plugins.FormatterQuirks.Psi.CodeStyle.Formatting
{
#if net222
  [Language(typeof(CSharpLanguage))]
  public class QuirkyFormatterIntAlignPredicate : ICustomShouldDoIntAlignPredicate
  {
    private readonly ISettingsStore mySettingsStore;

    public QuirkyFormatterIntAlignPredicate(ISettingsStore settingsStore)
    {
      mySettingsStore = settingsStore;
    }

    public bool Check() =>
      mySettingsStore.BindToContextTransient(ContextRange.ApplicationWide)
        .GetValue((QuirkyFormattingSettingsKey x) => x.INT_ALIGN_ATTRIBUTE_COMMAS);
  }
#endif

  [Language(typeof(CSharpLanguage))]
  public class QuirkyCSharpFormatterInfoProvider : CSharpFormatterInfoProviderPart
  {
    private const int WeakQuirkyPriority = 99_999;
    private const int QuirkyPriority = 100_000;

    public QuirkyCSharpFormatterInfoProvider(
      ISettingsSchema settingsSchema,
      ICalculatedSettingsSchema calculatedSettingsSchema,
      IThreading threading, Lifetime lifetime
    ) : base(settingsSchema, calculatedSettingsSchema, threading, lifetime)
    {
    }

    protected override void Initialize()
    {
      base.Initialize();

      // Put the descriptions of the rules here

      // Some possible rule types to put into Describe<TRuleType> or DescribeWithExternalKey<TKey, TRuleType>:
      // FormattingRule  -> To describe a calculation of line breaks between some two tree nodes.
      // ElementListRule -> To describe how a sequence of elements should be formatted (i.e. the list in "int a, b, c = 3, d;") 
      // IntAlignRule    -> To describe some kind of column-based formatting
      // ...

      QuirkyLineBreaks();
      QuirkyIntAlign();
    }

    private void QuirkyLineBreaks()
    {
      // This rule turns off the enforcement of linebreaks between statements.
      // Its priority must be lower than any rule's that also modifies statements' line breaks behavior
      DescribeWithExternalKey<QuirkyFormattingSettingsKey, FormattingRule>()
        .Group(LineBreaksRuleGroup)
        .Name("ENFORCE_LINE_BREAKS_BETWEEN_STATEMENTS")
        .Where(
          Left().In(ElementBitsets.C_SHARP_STATEMENT_BIT_SET),
          Right().In(ElementBitsets.C_SHARP_STATEMENT_BIT_SET)
        )
        .SwitchOnExternalKey(x => x.ENFORCE_LINE_BREAKS_BETWEEN_STATEMENTS,
          When(false).Return(IntervalFormatType.OnlySpace)
        )
        .Priority(WeakQuirkyPriority)
        .Build();

      // Don't do linebreaks after braces to get the lisp-style effect
      DescribeWithExternalKey<QuirkyFormattingSettingsKey, FormattingRule>()
        .Group(LineBreaksRuleGroup)
        .Name("NO_LINEBREAKS_AFTER_LBRACE")
        .Where(
          Left().HasType(CSharpTokenType.LBRACE),
          Right().In(ElementBitsets.C_SHARP_STATEMENT_BIT_SET)
        )
        .SwitchOnExternalKey(x => x.ENFORCE_LINE_BREAKS_AFTER_LEFT_BRACES,
          When(false).Return(IntervalFormatType.OnlySpace)
        )
        .Priority(QuirkyPriority)
        .Build();

      // Indent last brace to get the banner-style effect
      DescribeWithExternalKey<QuirkyFormattingSettingsKey, IndentingRule>()
        .Group(LineBreaksRuleGroup)
        .Name("INDENT_RBRACE")
        .Where(
          Parent().HasType(ElementType.BLOCK),
          Left().In(ElementBitsets.C_SHARP_STATEMENT_BIT_SET),
          Right().HasType(CSharpTokenType.RBRACE)
        )
        .SwitchOnExternalKey(x => x.BANNER_STYLE_RIGHT_BRACE,
          When(true).Switch(
            x => x.CONTINUOUS_INDENT_MULTIPLIER,
            ContinuousIndentRule.ContinuousIndentOptions(this)))
        .Priority(QuirkyPriority)
        .Build();

      // The following rule demonstrates the use of custom predicates to detect the proper formatting context.
      DescribeWithExternalKey<QuirkyFormattingSettingsKey, FormattingRule>()
        .Group(LineBreaksRuleGroup)
        .Name("LOCAL_FUNCTION_DECLARATION_AND_INVOCATION_LINEBREAKS")
        .Where(
          Left().Satisfies((node, _) =>
            node is IExpressionStatement expressionStatement &&
            expressionStatement.HasLocalFunctionInvocation()),
          Right().Satisfies((node, _) =>
            node is IDeclarationStatement { LocalFunctionDeclaration: { } localFunctionDeclaration }
            && node.LeftSiblings().OfType<IExpressionStatement>().FirstOrDefault() is { } expressionStatement
            && expressionStatement.GetInvokedLocalFunctionDeclarations().Contains(localFunctionDeclaration))
        )
        .SwitchOnExternalKey(x => x.ENFORCE_LOCAL_FUNCTION_DECLARATION_AND_INVOCATION_LINEBREAKS,
          When(true).Return(IntervalFormatType.NewLine),
          When(false).Return(IntervalFormatType.OnlySpace)
        )
        .Priority(QuirkyPriority)
        .Build();
    }

    private void QuirkyIntAlign()
    {
      // This rule is to demonstrate how the IntAlignRules must be written.
      // It binds an IntAlign token to the formatting context defined in the Where() clause.
      // The token serves as a stretcher that pads with whitespace to the left so there would be no outdent.
      // Token id equality is extremely important. The columns are defined in terms of it.
      DescribeWithExternalKey<QuirkyFormattingSettingsKey, IntAlignRule>()
        .Name("ALIGN_COMMAS_IN_ATTRIBUTE_INVOCATIONS")
        .Where(
          Parent().Satisfies((node, _) => node is IAttribute),
          Right().HasType(CSharpTokenType.COMMA)
        )
        .SwitchOnExternalKey(x => x.INT_ALIGN_ATTRIBUTE_COMMAS,
          When(true).Calculate((formattingRangeContext, context) =>
          {
            if (formattingRangeContext == null && context == null) return null;

            var attribute = (IAttribute)((FormattingRangeContext)formattingRangeContext)?.Parent;

            if (attribute?.Name == null) return null;

            return new IntAlignOptionValue(
              $"Comma${attribute.Name.GetText()}$StartingAtOffset{attribute.GetDocumentStartOffset().Offset}",
              QuirkyPriority
            );
          })
        )
        .Build();
    }
#if net223
    public override IEnumerable<IScalarSetting<bool>> PureIntAlignSettings()
    {
      yield return CalculatedSettingsSchema.GetScalarSetting((QuirkyFormattingSettingsKey x) => x.INT_ALIGN_ATTRIBUTE_COMMAS);
    }
#endif
  }
}