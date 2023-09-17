using JetBrains.Application.BuildScript.Application.Zones;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Resources.Shell;

namespace JetBrains.ReSharper.Plugins.QuirkyFormatting;

[ZoneDefinition]
public interface IQuirkyFormattingZone
    : IZone, IRequire<ILanguageCSharpZone>, IRequire<PsiFeaturesImplZone>
{
}