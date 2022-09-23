using JetBrains.Application.BuildScript.Application.Zones;
using JetBrains.Application.Environment;
using JetBrains.ReSharper.Plugins.FormatterQuirks;

// Must be in the global namespace
[ZoneActivator] public class ZoneActivator : IActivate<IFormatterQuirksZone> { }

namespace JetBrains.ReSharper.Plugins.FormatterQuirks
{
  [ZoneMarker] public class ZoneMarker : IRequire<IFormatterQuirksZone> { }
}