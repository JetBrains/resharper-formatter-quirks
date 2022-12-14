rootProject.name = "formatter-quirks"

pluginManagement {
    repositories {
        maven { setUrl("https://cache-redirector.jetbrains.com/plugins.gradle.org") }
        maven { setUrl("https://oss.sonatype.org/content/repositories/snapshots/") }
    }
    resolutionStrategy {
        eachPlugin {
            when(requested.id.name) {
                "rdgen" -> { useModule("com.jetbrains.rd:rd-gen:${requested.version}") }
            }
        }
    }
}