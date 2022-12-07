using JetBrains.Application.BuildScript.Application.Zones;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Resources.Shell;

namespace JetBrains.ReSharper.Plugins.QuirkyFormattings
{
  [ZoneDefinition]
  public interface IQuirkyFormattingsZone
    : IZone,
      IRequire<ILanguageCSharpZone>,
      IRequire<PsiFeaturesImplZone>
  { }
}