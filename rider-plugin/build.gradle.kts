import com.ullink.NuGetPack
import org.apache.tools.ant.taskdefs.condition.Os
import org.jetbrains.intellij.tasks.PrepareSandboxTask


val productVersion: String by project
val pluginVersion: String by project
val projectDirPath: String = project.projectDir.invariantSeparatorsPath
val riderSdkPath = "${projectDirPath}/build/riderRD-${productVersion}"
val dotnetSdkPath = "$riderSdkPath/lib/DotNetSdkForRdPlugins"
val dotnetSdkPropsPath = "$dotnetSdkPath/Build"
val bundledMavenArtifactsPath = "${projectDirPath}/build/maven-artifacts"
val skipDotNet: String by project

val buildConfiguration: String by project
val dotnetVersion: String by project
val waveId = productVersion.slice(2..3) + productVersion[5] // 2022.2.x -> 222
val werror: String by project

val dotnetDllFiles = listOf(
        "../resharper-plugin/FormatterQuirks/bin/$buildConfiguration/net472/JetBrains.ReSharper.Plugins.FormatterQuirks.dll",
        "../resharper-plugin/FormatterQuirks/bin/$buildConfiguration/net472/JetBrains.ReSharper.Plugins.FormatterQuirks.pdb"
)


plugins {
    id("org.jetbrains.intellij") version "1.9.0"
    kotlin("jvm") version "1.6.10"

    id("com.ullink.nunit") version "2.4"
    id("com.ullink.nuget") version "2.23"
}

repositories {
    maven { setUrl("https://cache-redirector.jetbrains.com/maven-central") }
}

intellij {
    pluginName.set("formatter-quirks")
    type.set("RD")

    version.set(productVersion)
    downloadSources.set(false)

    plugins.set(listOf("rider-plugins-appender"))
}

tasks {
    runIde {
        maxHeapSize = "1500m"
    }

    val generateNuGetConfig by registering {
        file("../nuget.config").writeText("""
            |<?xml version="1.0" encoding="utf-8"?>
            |<!-- This file was generated by the `generateNuGetConfig` task of build.gradle.kts -->
            |<configuration>
            |  <packageSources>
            |    <clear />
            |    <add key="local-dotnet-sdk" value="$dotnetSdkPath" />
            |    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
            |  </packageSources>
            |</configuration>
        """.trimMargin())
    }

    val generateMsBuildPropsFromGradleBuild by registering {
        file("../resharper-plugin/BuildState.Generated.props").writeText("""
            |<!-- This file was generated by the `generateMsBuildPropsFromGradleBuild` task of build.gradle.kts -->
            |<Project>
            |  <PropertyGroup>
            |    <ProductVersion>$productVersion</ProductVersion>
            |    <DotNetSdkProps>$dotnetSdkPropsPath</DotNetSdkProps>
            |  </PropertyGroup>
            |</Project>
        """.trimMargin())
    }

    fun findDotNet(): String? {
        if (project.extra.has("dotnetCli")) {
            val dotnetCliCandidate = project.extra["dotnetCli"] as? String
            if (File(dotnetCliCandidate).exists()) return dotnetCliCandidate
        }

        for (dir in System.getenv("PATH").split(File.pathSeparatorChar)) {
            val dotnetCli = File(dir, if (Os.isFamily(Os.FAMILY_WINDOWS)) "dotnet.exe" else "dotnet")
            if (dotnetCli.exists()) {
                project.extra["dotnetCli"] = dotnetCli.canonicalPath
                return dotnetCli.canonicalPath
            }
        }

        return null
    }

    val cleanPlugin by registering {
        doLast {
            delete("build")
            delete("../resharper-plugin/BuildState.Generated.props")
            delete("../nuget.config")
            delete("../resharper-plugin/test/data/JetTestPackages")
        }
    }

    val runDotnetRestore by registering {
        val resharperDir =  File(project.projectDir, "../resharper-plugin/")
        val dotnetCli = findDotNet() ?: error("No `dotnet` in the PATH variable")
        val dotnetArgs = listOf("restore")

        doLast {
            logger.info("dotnet call: `$dotnetCli` `$dotnetArgs` in `${resharperDir}`")
            project.exec {
                executable = dotnetCli
                args = dotnetArgs
                workingDir = resharperDir
            }
        }
    }

    val prepareDotnetPart by registering {
        dependsOn(generateNuGetConfig, generateMsBuildPropsFromGradleBuild, runDotnetRestore)
    }

    val buildDotnet by registering {
        dependsOn(prepareDotnetPart)
        shouldRunAfter(buildPlugin)
        val solution = File(project.projectDir, "../resharper-plugin/FormatterQuirks.sln")

        val dotnetCli = findDotNet() ?: error("No `dotnet` in the PATH variable")
        val dotnetArgs = listOf(
            "build",
            solution.canonicalPath,
            "/p:Configuration=$buildConfiguration",
            "/p:Version=${pluginVersion}",
            "/p:TreatWarningsAsErrors=${werror}",
            "/v:${
                when (project.gradle.startParameter.logLevel) {
                    LogLevel.QUIET -> "quiet"
                    LogLevel.LIFECYCLE -> "minimal"
                    LogLevel.INFO -> "normal"
                    LogLevel.DEBUG -> "detailed"
                    else -> "normal"
                }
            }",
            "/bl:${solution.name + ".binlog"}",
            "/nologo"
        )

        doLast {
            if (skipDotNet == "true") return@doLast
            logger.info("dotnet call: `$dotnetCli` `$dotnetArgs` in `${solution.parentFile}`")
            project.exec {
                executable = dotnetCli
                args = dotnetArgs
                workingDir = solution.parentFile
            }
        }
    }

    withType<PrepareSandboxTask> {
        dependsOn(buildDotnet)
        dotnetDllFiles.forEach { from(it) { into("${intellij.pluginName.get()}/dotnet") } }
    }

    withType<org.jetbrains.intellij.tasks.RunIdeTask> {
        maxHeapSize = "1500m"
        jvmArgs = listOf(
            "-Drider.backend.netcore=false"
        )
    }

    val pluginNugetPack by registering(NuGetPack::class) {
        dependsOn(buildDotnet)
        packageAnalysis = false
        packageVersion = pluginVersion

        properties = mapOf(
            "plugin_version" to pluginVersion,
            "configuration" to buildConfiguration.toLowerCase(),
            "dotnet_version" to dotnetVersion,
            "wave_id" to waveId
        )

        setNuspecFile("${project.projectDir}/../resharper-plugin/formatter-quirks.nuspec")
        setDestinationDir("${project.projectDir}/build/distributions/$buildConfiguration")
    }
}