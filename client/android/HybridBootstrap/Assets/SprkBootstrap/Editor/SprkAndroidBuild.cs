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
///             -executeMethod SprkAndroidBuild.Build -logFile ../sprk-android/bootstrap/build.log
/// </summary>
public static class SprkAndroidBuild
{
    const string ScenePath = "Assets/Scenes/Boot.unity";
    const string HotDir = "HotUpdate";
    const string Streaming = "Assets/StreamingAssets/sprk";

    static string ProjectDir => Path.GetDirectoryName(Application.dataPath);

    // Build output lives with the other generated files: ../sprk-android/bootstrap (next to the repository, outside artifacts/).
    public static string ArtifactsDir => Path.GetFullPath(Path.Combine(ProjectDir, "..", "..", "..", "..", "sprk-android", "bootstrap"));

    static List<string> HotNames() =>
        File.ReadAllLines(Path.Combine(ProjectDir, HotDir, "hot-update.txt")).Select(l => l.Trim()).Where(l => l.Length > 0).ToList();

    public static void Configure()
    {
        var android = NamedBuildTarget.Android;
        PlayerSettings.companyName = "SPRK";
        // Same name and icon as the Windows game. The package id stays: changing it would be a
        // new app on the device and lose the installed patch data (8.6 GB) and saved logins.
        PlayerSettings.productName = "King's Raid";
        PlayerSettings.SetApplicationIdentifier(android, "com.sprk.hybridprobe");
        ApplyAppIcon();
        // No "Made with Unity" splash (optional on Unity 6 Personal).
        PlayerSettings.SplashScreen.show = false;
        PlayerSettings.SplashScreen.showUnityLogo = false;
        PlayerSettings.SetScriptingBackend(android, ScriptingImplementation.IL2CPP);
        PlayerSettings.SetApiCompatibilityLevel(android, ApiCompatibilityLevel.NET_Unity_4_8);
        PlayerSettings.SetManagedStrippingLevel(android, ManagedStrippingLevel.Minimal);
        PlayerSettings.stripEngineCode = false;
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel23;
        // Landscape on either side, following the device's auto-rotate setting.
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
        PlayerSettings.allowedAutorotateToLandscapeLeft = true;
        PlayerSettings.allowedAutorotateToLandscapeRight = true;
        PlayerSettings.allowedAutorotateToPortrait = false;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
        PlayerSettings.useAnimatedAutorotation = true;
        PlayerSettings.gcIncremental = false; // HybridCLR: keep the simplest GC mode for the first proof
        PlayerSettings.SetIl2CppCompilerConfiguration(android, Il2CppCompilerConfiguration.Release);
        PlayerSettings.SetIl2CppCodeGeneration(android, UnityEditor.Build.Il2CppCodeGeneration.OptimizeSize);
        EditorUserBuildSettings.buildAppBundle = false;
        // The original Android bundles (Unity 2018) only carry OpenGL ES shader variants;
        // under Vulkan Unity fails "shader compiler platform 18 not in blob" and then crashes.
        PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
        PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3 });
        PlayerSettings.colorSpace = ColorSpace.Gamma; // matches the game's player settings
        ApplyGameProjectSettings();

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

    // Values read from the game's globalgamemanagers (TagManager). Repacked scenes and
    // prefabs store tag/layer indices, so the Android player must define the same ones.
    static readonly string[] GameTags = { "AnimationRoot", "BroadcastCamera" };
    static readonly string[] GameLayers =
    {
        "Default", "TransparentFX", "Ignore Raycast", "", "Water", "UI", "", "", "Hero3DPreview", "GlobalUI",
        "BattleBackground3d", "BattleBackgroundUI", "BottomFixedUI", "BottomUI", "Bottom3D", "CutSceneUI",
        "CutScene3D", "CutSceneBottomUI", "Hero3DPreview2", "Hero3DPreview3", "Hero3DPreview4", "Hero3DPreview5",
        "BroadcastUI", "Global3D", "GlobalVideo", "UI3D", "UI3DUI", "Bottom3DUI", "BattleUI", "SpineUI", "Pet", "",
    };

    // Built-in shaders the game content uses that only exist in the player's builtin_extra.
    // The legacy transparent shaders are referenced by asset bundles converted from Windows
    // (client/android/HybridBootstrap/content/convert_bundles.py).
    static readonly string[] IncludedBuiltinShaders = { "Legacy Shaders/Diffuse", "Unlit/Texture", "Unlit/Transparent",
        "Legacy Shaders/Transparent/Diffuse", "Legacy Shaders/Transparent/VertexLit" };

    static void ApplyGameProjectSettings()
    {
        var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        var tags = tagManager.FindProperty("tags");
        tags.ClearArray();
        for (int i = 0; i < GameTags.Length; i++) { tags.InsertArrayElementAtIndex(i); tags.GetArrayElementAtIndex(i).stringValue = GameTags[i]; }
        var layers = tagManager.FindProperty("layers");
        for (int i = 0; i < GameLayers.Length; i++) layers.GetArrayElementAtIndex(i).stringValue = GameLayers[i];
        tagManager.ApplyModifiedPropertiesWithoutUndo();

        var graphics = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset")[0]);
        var always = graphics.FindProperty("m_AlwaysIncludedShaders");
        var present = Enumerable.Range(0, always.arraySize).Select(i => always.GetArrayElementAtIndex(i).objectReferenceValue).ToList();
        foreach (var name in IncludedBuiltinShaders)
        {
            var shader = Shader.Find(name);
            if (shader == null) { Debug.Log($"[SprkAndroidBuild] built-in shader not available: {name}"); continue; }
            if (present.Contains(shader)) continue;
            always.InsertArrayElementAtIndex(always.arraySize);
            always.GetArrayElementAtIndex(always.arraySize - 1).objectReferenceValue = shader;
        }
        graphics.ApplyModifiedPropertiesWithoutUndo();
    }

    static void WriteLinkXml()
    {
        // Interpreted game code may call anything in the third-party assemblies.
        var plugins = Directory.GetFiles(Path.Combine(Application.dataPath, "Plugins/GameRuntime"), "*.dll")
            .Select(Path.GetFileNameWithoutExtension).OrderBy(n => n);
        // Engine modules too: the game code is interpreted, so the linker cannot see which
        // engine methods it calls and strips them (e.g. Debug.DrawRay -> MissingMethodException
        // in the world-map input handler, which froze map drag/zoom/taps).
        string engineDir = Path.Combine(EditorApplication.applicationContentsPath, "Managed", "UnityEngine");
        var engine = Directory.GetFiles(engineDir, "UnityEngine*.dll").Select(Path.GetFileNameWithoutExtension)
            .Where(n => n != "UnityEngine" && !n.Contains("Editor")).OrderBy(n => n);
        var xml = "<linker>\n" + string.Concat(plugins.Concat(engine).Select(n => $"  <assembly fullname=\"{n}\" preserve=\"all\"/>\n")) + "</linker>\n";
        File.WriteAllText(Path.Combine(Application.dataPath, "SprkBootstrap/link.xml"), xml);
        AssetDatabase.ImportAsset("Assets/SprkBootstrap/link.xml");
    }

    const string IconPath = "Assets/SprkBootstrap/Generated/AppIcon.png"; // written by sync_inputs.py

    static void ApplyAppIcon()
    {
        AssetDatabase.ImportAsset(IconPath);
        var icon = AssetDatabase.LoadAssetAtPath<Texture2D>(IconPath);
        if (icon == null) throw new Exception($"{IconPath} missing; run sync_inputs.py");
        PlayerSettings.SetIcons(NamedBuildTarget.Unknown, new[] { icon }, IconKind.Any);
    }

    static void EnsureScene()
    {
        Directory.CreateDirectory(Path.Combine(Application.dataPath, "Scenes"));
        // A black frame while the game code and content load: no skybox, light or debug text.
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        RenderSettings.skybox = null;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = Color.black;
        var cam = new GameObject("BootCamera").AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        cam.cullingMask = 0;
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

        string apk = Path.Combine(ArtifactsDir, "apk", "sprk-hybrid-probe.apk");
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
        // The repacked built-in content goes into the APK, so an installed APK starts the game
        // on its own (a device-pushed copy still takes precedence; SprkBootstrap.LoadContent).
        index = index.Concat(StageContent().Select(f => "content:" + f)).Concat(new[] { "start:Login" });
        StageLoginAtlas();
        File.WriteAllLines(Path.Combine(ProjectDir, Streaming, "index.txt"), index);
        // Verifies servers' signed release manifests (SprkUpdater). Same key as SprkLauncher.
        File.Copy(Path.Combine(ProjectDir, "../../../config/client-update-public.pem"),
                  Path.Combine(ProjectDir, Streaming, "update-public.pem"), true);
        AssetDatabase.Refresh();
        Debug.Log($"[SprkAndroidBuild] staged {aot.Count} AOT metadata assemblies");
    }

    // Mount order: shaders before anything that references them, scenes last.
    static readonly string[] ContentFiles = { "sprk-shaders.unity3d", "sprk-ggm.unity3d", "sprk-resources.unity3d", "sprk-scenes.unity3d" };

    // content/repack_content.py output -> StreamingAssets/sprk/content (game data: the
    // StreamingAssets folder is ignored by git). Unchanged files are not copied again.
    static List<string> StageContent()
    {
        string src = Path.Combine(ArtifactsDir, "content");
        string dst = Path.Combine(ProjectDir, Streaming, "content");
        Directory.CreateDirectory(dst);
        foreach (var f in ContentFiles)
        {
            var from = new FileInfo(Path.Combine(src, f));
            if (!from.Exists) throw new Exception($"{from.FullName} missing; run client/android/dev.py content");
            var to = new FileInfo(Path.Combine(dst, f));
            if (!to.Exists || to.Length != from.Length || to.LastWriteTimeUtc != from.LastWriteTimeUtc)
            {
                from.CopyTo(to.FullName, true);
                File.SetLastWriteTimeUtc(to.FullName, from.LastWriteTimeUtc);
            }
        }
        foreach (var stale in Directory.GetFiles(dst).Where(p => !ContentFiles.Contains(Path.GetFileName(p)) && !p.EndsWith(".meta")))
        {
            File.Delete(stale);
        }
        return ContentFiles.ToList();
    }

    // The title logo's atlas (with the "X: The Final" subtitle) from the maintained Android
    // asset set, so the title is complete before the first asset download
    // (SprkLoginPresentation.RestoreLogoSubtitle falls back to this copy).
    static void StageLoginAtlas()
    {
        const string name = "NguiAtlas_UI_Prefab_Login.unity3d";
        foreach (var spec in new[] { "HighAndMidSpec", "LowLargeSpec" })
        {
            string src = Path.GetFullPath(Path.Combine(ProjectDir, "..", "..", "..", "..", "sprk-android", "build", "Assetbundle", spec, name));
            if (!File.Exists(src))
            {
                throw new Exception($"{src} missing; run client/android/dev.py assets");
            }
            string dst = Path.Combine(ProjectDir, Streaming, "login", spec, name);
            Directory.CreateDirectory(Path.GetDirectoryName(dst));
            File.Copy(src, dst, true);
        }
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
