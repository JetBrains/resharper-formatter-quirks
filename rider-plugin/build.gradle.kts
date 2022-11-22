import org.apache.tools.ant.taskdefs.condition.Os

plugins {
    id("com.ullink.nuget") version "2.23"

    id("org.jetbrains.intellij") version "1.9.0"
    kotlin("jvm") version "1.7.0"
}

repositories {
    maven { setUrl("https://cache-redirector.jetbrains.com/maven-central") }
    maven { setUrl("https://cache-redirector.jetbrains.com/intellij-dependencies") }
    maven { setUrl("https://cache-redirector.jetbrains.com/www.jetbrains.com/intellij-repository/snapshots") }
}

val sdkVersion = "2022.3-SNAPSHOT"
val pluginVersion = "2022.3"

val skipDotNet = false
val projectDirPath = projectDir.invariantSeparatorsPath

val buildConfiguration = "RELEASE"
val dotnetVersion = "net472"

val riderSdkPath = "${projectDirPath}/build/riderRD-${sdkVersion}"
val dotnetSdkPath = "$riderSdkPath/lib/DotNetSdkForRdPlugins"
val dotnetSdkPropsPath = "$dotnetSdkPath/Build"
val bundledMavenArtifactsPath = "${projectDirPath}/build/maven-artifacts"

val waveId = sdkVersion.slice(2..3) + sdkVersion[5] // 2022.2.x -> 222
val werror = false

val dotnetDllFiles = listOf(
    "../resharper-plugin/FormatterQuirks/bin/$buildConfiguration/net472/JetBrains.ReSharper.Plugins.FormatterQuirks.dll",
    "../resharper-plugin/FormatterQuirks/bin/$buildConfiguration/net472/JetBrains.ReSharper.Plugins.FormatterQuirks.pdb"
)

intellij {
    pluginName.set("formatter-quirks")

    downloadSources.set(false)
    instrumentCode.set(false)

    type.set("RD")
    version.set(sdkVersion)

    plugins.set(listOf("rider-plugins-appender"))
}

tasks {
    withType<org.jetbrains.intellij.tasks.RunIdeTask> {
        maxHeapSize = "1500m"
        jvmArgs = listOf(
            "-Drider.backend.netcore=false"
        )
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
            |    <ProductVersion>$sdkVersion</ProductVersion>
            |    <DotNetSdkProps>$dotnetSdkPropsPath</DotNetSdkProps>
            |    <DefineConstants>net$waveId</DefineConstants>
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
            if (skipDotNet) return@doLast
            logger.info("dotnet call: `$dotnetCli` `$dotnetArgs` in `${solution.parentFile}`")
            project.exec {
                executable = dotnetCli
                args = dotnetArgs
                workingDir = solution.parentFile
            }
        }
    }

    withType<org.jetbrains.intellij.tasks.PrepareSandboxTask> {
        dependsOn(buildDotnet)
        dotnetDllFiles.forEach { from(it) { into("${intellij.pluginName.get()}/dotnet") } }
    }

    val nuGetPack by registering(com.ullink.NuGetPack::class) {
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