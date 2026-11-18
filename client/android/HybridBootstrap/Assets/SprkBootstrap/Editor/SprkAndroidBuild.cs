using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HybridCLR;
using HybridCLR.Commands;
using HybridCLR.Installer;
using HybridCLR.Settings;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Batch-mode entry points. Run from the repository root with:
///   Unity.exe -batchmode -quit -projectPath client/android/HybridBootstrap -buildTarget Android
///             -executeMethod SprkAndroidBuild.Build -logFile artifacts/android-bootstrap/build.log
/// </summary>
public static class SprkAndroidBuild
{
    const string ScenePath = "Assets/Scenes/Boot.unity";
    const string HotDir = "HotUpdate";
    const string Streaming = "Assets/StreamingAssets/sprk";

    static string ProjectDir => Path.GetDirectoryName(Application.dataPath);

    static List<string> HotNames() =>
        File.ReadAllLines(Path.Combine(ProjectDir, HotDir, "hot-update.txt")).Select(l => l.Trim()).Where(l => l.Length > 0).ToList();

    public static void Configure()
    {
        var android = NamedBuildTarget.Android;
        PlayerSettings.companyName = "SPRK";
        PlayerSettings.productName = "SPRK Hybrid Probe";
        PlayerSettings.SetApplicationIdentifier(android, "com.sprk.hybridprobe");
        PlayerSettings.SetScriptingBackend(android, ScriptingImplementation.IL2CPP);
        PlayerSettings.SetApiCompatibilityLevel(android, ApiCompatibilityLevel.NET_Unity_4_8);
        PlayerSettings.SetManagedStrippingLevel(android, ManagedStrippingLevel.Minimal);
        PlayerSettings.stripEngineCode = false;
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel23;
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
        PlayerSettings.gcIncremental = false; // HybridCLR: keep the simplest GC mode for the first proof
        PlayerSettings.SetIl2CppCompilerConfiguration(android, Il2CppCompilerConfiguration.Release);
        PlayerSettings.SetIl2CppCodeGeneration(android, UnityEditor.Build.Il2CppCodeGeneration.OptimizeSize);
        EditorUserBuildSettings.buildAppBundle = false;
        // The original Android bundles (Unity 2018) only carry OpenGL ES shader variants;
        // under Vulkan Unity fails "shader compiler platform 18 not in blob" and then crashes.
        PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
        PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3 });

        var s = HybridCLRSettings.instance;
        s.enable = true;
        s.hybridclrRepoURL = "https://github.com/focus-creative-games/hybridclr";
        s.il2cppPlusRepoURL = "https://github.com/focus-creative-games/il2cpp_plus";
        s.hotUpdateAssemblies = HotNames().ToArray();
        s.externalHotUpdateAssembliyDirs = new[] { HotDir };
        HybridCLRSettings.Save();

        WriteLinkXml();
        EnsureScene();
        AssetDatabase.SaveAssets();
        Debug.Log("[SprkAndroidBuild] configured");
    }

    static void WriteLinkXml()
    {
        // Interpreted game code may call anything in the third-party assemblies.
        var plugins = Directory.GetFiles(Path.Combine(Application.dataPath, "Plugins/GameRuntime"), "*.dll")
            .Select(Path.GetFileNameWithoutExtension).OrderBy(n => n);
        var xml = "<linker>\n" + string.Concat(plugins.Select(n => $"  <assembly fullname=\"{n}\" preserve=\"all\"/>\n")) + "</linker>\n";
        File.WriteAllText(Path.Combine(Application.dataPath, "SprkBootstrap/link.xml"), xml);
        AssetDatabase.ImportAsset("Assets/SprkBootstrap/link.xml");
    }

    static void EnsureScene()
    {
        Directory.CreateDirectory(Path.Combine(Application.dataPath, "Scenes"));
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var go = new GameObject("SprkBootstrap");
        go.AddComponent<SprkBootstrap>();
        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
    }

    public static void InstallHybridCLR()
    {
        var installer = new InstallerController();
        if (!installer.HasInstalledHybridCLR())
        {
            installer.InstallDefaultHybridCLR();
        }
        if (!installer.HasInstalledHybridCLR()) throw new Exception("HybridCLR install failed");
        Debug.Log($"[SprkAndroidBuild] HybridCLR installed: {installer.InstalledLibil2cppVersion}");
    }

    public static void Build()
    {
        Configure();
        InstallHybridCLR();
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            throw new Exception("run with -buildTarget Android");

        PrebuildCommand.GenerateAll();
        StageStreamingAssets();

        string apk = Path.Combine(ProjectDir, "Build", "sprk-hybrid-probe.apk");
        Directory.CreateDirectory(Path.GetDirectoryName(apk));
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = new[] { ScenePath },
            target = BuildTarget.Android,
            locationPathName = apk,
            options = BuildOptions.None,
        });
        Debug.Log($"[SprkAndroidBuild] result={report.summary.result} size={report.summary.totalSize} errors={report.summary.totalErrors} -> {apk}");
        if (report.summary.result != BuildResult.Succeeded) throw new Exception("build failed");
    }

    static void StageStreamingAssets()
    {
        string aotSrc = SettingsUtil.GetAssembliesPostIl2CppStripDir(BuildTarget.Android);
        string aotDst = Path.Combine(ProjectDir, Streaming, "aot");
        if (Directory.Exists(aotDst)) Directory.Delete(aotDst, true);
        Directory.CreateDirectory(aotDst);

        // Supplement metadata for every AOT assembly HybridCLR reports as needing it.
        var patched = AOTGenericReferencesList();
        var aot = new List<string>();
        foreach (var name in patched)
        {
            string src = Path.Combine(aotSrc, name + ".dll");
            if (!File.Exists(src)) { Debug.LogWarning($"[SprkAndroidBuild] no stripped AOT dll for {name}"); continue; }
            File.Copy(src, Path.Combine(aotDst, name + ".dll.bytes"), true);
            aot.Add(name);
        }

        var index = aot.Select(n => "aot:" + n).Concat(HotNames().Select(n => "hot:" + n));
        string bundles = Path.Combine(ProjectDir, HotDir, "bundles.txt");
        if (File.Exists(bundles)) index = index.Concat(File.ReadAllLines(bundles).Where(l => l.Trim().Length > 0).Select(l => "bundle:" + l.Trim()));
        File.WriteAllLines(Path.Combine(ProjectDir, Streaming, "index.txt"), index);
        AssetDatabase.Refresh();
        Debug.Log($"[SprkAndroidBuild] staged {aot.Count} AOT metadata assemblies");
    }

    static List<string> AOTGenericReferencesList()
    {
        // Generated by GenerateAll: PatchedAOTAssemblyList = new List<string> { "mscorlib.dll", ... }
        string file = Path.Combine(ProjectDir, "Assets", HybridCLRSettings.instance.outputAOTGenericReferenceFile);
        var names = new List<string>();
        bool inList = false;
        foreach (var raw in File.ReadAllLines(file))
        {
            var l = raw.Trim();
            if (l.Contains("PatchedAOTAssemblyList")) inList = true;
            else if (inList && l.StartsWith("}")) break;
            else if (inList && l.StartsWith("\"")) names.Add(Path.GetFileNameWithoutExtension(l.Trim('"', ',', ' ')));
        }
        return names;
    }
}
