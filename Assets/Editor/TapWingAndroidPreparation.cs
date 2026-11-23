using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

public static class TapWingAndroidPreparation
{
    public static void CompilePlayer()
    {
        const string output = "../TapWing-Backups/PlayerScripts";
        Directory.CreateDirectory(output);
        var result = UnityEditor.Build.Player.PlayerBuildInterface.CompilePlayerScripts(
            new UnityEditor.Build.Player.ScriptCompilationSettings {
                group = BuildTargetGroup.Standalone,
                target = BuildTarget.StandaloneWindows64
            }, output);
        if (result.assemblies == null || result.assemblies.Count == 0)
            throw new System.Exception("Player script compilation produced no assemblies.");
        Debug.Log("TAPWING_PLAYER_SCRIPTS_OK");
    }

    public static void Run()
    {
        // Keep the existing product/company pair so desktop PlayerPrefs keep their location.
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android,"com.diego.tapwing");
        AssetDatabase.SaveAssets();
        Directory.CreateDirectory("../TapWing-Backups/Reports");
        File.WriteAllText("../TapWing-Backups/Reports/android-preparation.txt",
            "Product: " + PlayerSettings.productName + "\n" +
            "Android ID: " + PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.Android) + "\n" +
            "Backend: " + PlayerSettings.GetScriptingBackend(NamedBuildTarget.Android) + "\n" +
            "Architectures: " + PlayerSettings.Android.targetArchitectures + "\n" +
            "Minimum SDK: " + PlayerSettings.Android.minSdkVersion + "\n" +
            "Target SDK: " + PlayerSettings.Android.targetSdkVersion + "\n" +
            "Android build module available: " + BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android,BuildTarget.Android) + "\n" +
            "No APK or device performance claim. Install Android Build Support before building.\n");
        Debug.Log("TAPWING_ANDROID_PREPARATION_OK");
    }
}
