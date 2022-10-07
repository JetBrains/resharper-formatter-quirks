# ReSharper quirky formatting

[![JetBrains official project](https://jb.gg/badges/official.svg)](https://confluence.jetbrains.com/display/ALL/JetBrains+on+GitHub)

This is a plugin that accumulates some of the quirkier formatting-related features. You can see the full list of user requests deemed *quirky enough* over at [ReSharper YouTrack](https://youtrack.jetbrains.com/issues?q=tag:%20format-plugin-candidate%20)

This Rider/ReSharper plugin consists of two parts:
* The rider part (`rider-plugin`): essentially a stub that sets up the ReSharper plugin for Rider and acts as a build bootstrap with its gradle configuration.
* The resharper part (`resharper-plugin`): the main part of the plugin that hosts much of the logic.

## Building the plugin 

### Requirements

* [.NET SDK 5](https://www.microsoft.com/net/download/windows) or newer
* [gradle](https://gradle.org/) build tool

### Optional

* [IntelliJ IDEA](https://www.jetbrains.com/idea/) (bundles `gradle`)
* [JetBrains Rider](https://www.jetbrains.com/rider/) or a different .NET IDE

### Build process

1. Either open the `rider-plugin` directory in IDEA or in some kind of a CLI that has `gradle` on its path.

2. Run the `:prepareDotnetPart` task.

3. Now, you can open the `resharper-plugin` project inside your favourite dotnet IDE (preferably, Rider :) ) and develop with pleasure!

### Getting the artifacts

In order to get a run-ready ReSharper plugin, you have to run the `:pluginNuGetPack` gradle task inside the `rider-plugin` project. The resulting `.nuget` artifact may be installed into ReSharper from a [custom package source](https://www.jetbrains.com/help/resharper/Managing_Extensions.html#add_package_source).

In order to run Rider with the installed plugin, you have to run the `:runIde` gradle task inside the `rider-plugin` project. It will make an apropriate IDEA sandbox through the [intellij gradle plugin](https://github.com/JetBrains/gradle-intellij-plugin) and launch the IDE.

In order to get a distribution of the Rider plugin, you have to run the `:buildPlugin` task inside the `ride    r-plugin` project.

You can change the build configuration (Debug/Release) in `gradle.properties`.

All artifacts are stored inside the `build` directory. Just look for them!