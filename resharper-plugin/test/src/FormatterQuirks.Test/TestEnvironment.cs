using System;
using System.IO;
using System.Threading;
using JetBrains.Application.BuildScript.Application.Zones;
using JetBrains.Application.Environment;
using JetBrains.ReSharper.TestFramework;
using JetBrains.TestFramework;
using JetBrains.TestFramework.Application.Zones;
using JetBrains.TestFramework.Utils;
using NUnit.Framework;

[assembly: Apartment(ApartmentState.STA)]

namespace JetBrains.ReSharper.Plugins.FormatterQuirks.Tests
{
  [ZoneMarker] public class ZoneMarker : IRequire<IFormatterQuirksZone> {}

  [ZoneActivator] public class PsiFeatureTestZoneActivator : IActivate<PsiFeatureTestZone> { }
  
  [ZoneActivator] public class FormatterQuirksZoneActivator : IActivate<IFormatterQuirksZone> { }
  
  [ZoneDefinition] public interface IFormatterQuirksTestsEnvZone 
    : ITestsEnvZone, IRequire<IFormatterQuirksZone>, IRequire<PsiFeatureTestZone> { }
  
  [SetUpFixture]
  public class TestEnvironment : ExtensionTestEnvironmentAssembly<IFormatterQuirksTestsEnvZone>
  {
    static TestEnvironment()
    {
      SetJetTestPackagesDir();
    }

    private static void SetJetTestPackagesDir()
    {
      if (Environment.GetEnvironmentVariable("JET_TEST_PACKAGES_DIR") != null) return;

      var packages = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        "JetTestPackages");
      if (!Directory.Exists(packages))
      {
        TestUtil.SetHomeDir(typeof(TestEnvironment).Assembly);
        var testData = TestUtil.GetTestDataPathBase(typeof(TestEnvironment).Assembly);
        packages = testData.Parent.Combine("JetTestPackages").FullPath;
      }

      if (!Directory.Exists(packages))
      {
        Directory.CreateDirectory(packages);
      }

      Environment.SetEnvironmentVariable("JET_TEST_PACKAGES_DIR", packages,
        EnvironmentVariableTarget.Process);
    }
  }
}