using JetBrains.Application.BuildScript.Application.Zones;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Resources.Shell;

namespace JetBrains.ReSharper.Plugins.FormatterQuirks
{
  [ZoneDefinition]
  public interface IFormatterQuirksZone
    : IZone,
      IRequire<ILanguageCSharpZone>,
      IRequire<PsiFeaturesImplZone>
  { }
}