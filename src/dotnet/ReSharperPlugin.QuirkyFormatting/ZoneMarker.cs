using JetBrains.Application.BuildScript.Application.Zones;
using JetBrains.Application.Environment;
using JetBrains.ReSharper.Plugins.QuirkyFormatting;

// Must be in the global namespace
[ZoneActivator] public class ZoneActivator : IActivate<IQuirkyFormattingZone> { }

namespace JetBrains.ReSharper.Plugins.QuirkyFormatting
{
    [ZoneMarker] public class ZoneMarker : IRequire<IQuirkyFormattingZone> { }
}