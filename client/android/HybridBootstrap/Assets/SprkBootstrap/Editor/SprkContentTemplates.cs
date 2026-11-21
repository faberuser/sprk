using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Builds tiny reference bundles with this exact Unity version and target so the
/// content repacker (content/repack_content.py, reading ../sprk-android/bootstrap/templates) can copy the AssetBundle object
/// layout, archive structure and engine type trees from real Unity output.
///   Unity.exe -batchmode -quit -projectPath client/android/HybridBootstrap -buildTarget Android
///             -executeMethod SprkContentTemplates.Build -logFile ../sprk-android/bootstrap/templates.log
/// </summary>
public static class SprkContentTemplates
{
    const string Dir = "Assets/SprkTemplate";

    public static void Build()
    {
        Directory.CreateDirectory(Dir);
        var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
        File.WriteAllBytes(Dir + "/tex.png", tex.EncodeToPNG());
        AssetDatabase.ImportAsset(Dir + "/tex.png");
        var mat = new Material(Shader.Find("Unlit/Texture")) { mainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(Dir + "/tex.png") };
        AssetDatabase.CreateAsset(mat, Dir + "/mat.mat");
        var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
        go.GetComponent<Renderer>().sharedMaterial = mat;
        go.AddComponent<SprkBootstrap>().enabled = false;
        PrefabUtility.SaveAsPrefabAsset(go, Dir + "/prefab.prefab");
        Object.DestroyImmediate(go);

        for (int i = 0; i < 2; i++)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            var g = new GameObject("TemplateObject" + i);
            g.AddComponent<MeshFilter>();
            g.AddComponent<MeshRenderer>().sharedMaterial = mat;
            EditorSceneManager.SaveScene(scene, $"{Dir}/Scene{i}.unity");
        }
        AssetDatabase.SaveAssets();

        string outDir = Path.Combine(SprkAndroidBuild.ArtifactsDir, "templates");
        Directory.CreateDirectory(outDir);
        var builds = new[]
        {
            new AssetBundleBuild { assetBundleName = "template_assets", assetNames = new[] { Dir + "/prefab.prefab", Dir + "/mat.mat", Dir + "/tex.png" } },
            new AssetBundleBuild { assetBundleName = "template_scenes", assetNames = new[] { Dir + "/Scene0.unity", Dir + "/Scene1.unity" } },
        };
        var opts = BuildAssetBundleOptions.UncompressedAssetBundle;
        var manifest = BuildPipeline.BuildAssetBundles(outDir, builds, opts, BuildTarget.Android);
        Debug.Log("[SprkContentTemplates] built: " + string.Join(",", manifest.GetAllAssetBundles()));
    }
}
