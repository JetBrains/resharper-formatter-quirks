using JetBrains.Application.BuildScript.Application.Zones;
using JetBrains.Application.Environment;
using JetBrains.ReSharper.Plugins.QuirkyFormattings;

// Must be in the global namespace
[ZoneActivator] public class ZoneActivator : IActivate<IQuirkyFormattingsZone> { }

namespace JetBrains.ReSharper.Plugins.QuirkyFormattings
{
  [ZoneMarker] public class ZoneMarker : IRequire<IQuirkyFormattingsZone> { }
}