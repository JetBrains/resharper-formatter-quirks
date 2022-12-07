using System;
using JetBrains.Application.platforms;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.TestFramework;
using JetBrains.TestFramework;
using JetBrains.Util.Dotnet.TargetFrameworkIds;

namespace JetBrains.ReSharper.Plugins.QuirkyFormattings.Tests
{
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
  public class TestQuirksAttribute : TestAspectAttribute, ITestTargetFrameworkIdProvider, ITestFileExtensionProvider
  {
    public string Extension => CSharpProjectFileType.CS_EXTENSION;

    public TargetFrameworkId GetTargetFrameworkId() => 
      TargetFrameworkId.Create(FrameworkIdentifier.NetFramework, new Version(4, 5, 1), ProfileIdentifier.Default);

    bool ITestTargetFrameworkIdProvider.Inherits => false;
  }
}