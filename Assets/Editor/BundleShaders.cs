/// Some code from https://bitbucket.org/Unity-Technologies/assetbundledemo/src/default/
#pragma warning disable 0618
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;

public static class BundleShaders {
    private static string assetBundleDirectory = "Assets/Editor/Output";
    private static string shaderDirectory = "Assets/Editor/Shaders";

    [MenuItem("Create Your Frisk/Build Shader AssetBundles...")]
    static void OneBundleOption() {
        if (EditorApplication.isPlaying) {
            Debug.LogError("You may only build AssetBundles while not in play mode.");
            return;
        }

        BundleShaderDialog window = (BundleShaderDialog)EditorWindow.GetWindow(typeof(BundleShaderDialog));
        window.Show();
    }

    public static void AllBundlesOption() {
        if (EditorApplication.isPlaying) {
            Debug.LogError("You may only build AssetBundles while not in play mode.");
            return;
        }

        // Gets all AssetBundles from files within the shader directory
        Dictionary<string, List<Shader>> bundles = RetrieveAllBundles();
        if (bundles == null)
            return;

        // Build AssetBundles
        BuildBundles(bundles);

        EditorUtility.DisplayDialog("Bundling Shaders", "All CYF Shader Bundles have been created!\n\nYou can find them in:\n" + assetBundleDirectory, "OK");
    }

    public static void OneBundle(string bundleName) {
        if (bundleName == "") {
            Debug.LogError("Please enter the name of an AssetBundle assigned in the Unity Editor.");
            return;
        }

        // Gets all AssetBundles from files within the shader directory
        Dictionary<string, List<Shader>> bundles = RetrieveAllBundles();
        if (bundles == null)
            return;

        // Check if the given bundle name exists within this dictionary
        if (!bundles.ContainsKey(bundleName)) {
            Debug.LogError("The AssetBundle \"" + bundleName + "\" does not exist on any files in \"" + shaderDirectory + "\".");
            return;
        }

        // Build all files with this bundle name into one AssetBundle
        Dictionary<string, List<Shader>> bundlesToBuild = new Dictionary<string, List<Shader>>();
        List<Shader> shaders = new List<Shader>();
        foreach (Shader shader in bundles[bundleName])
            shaders.Add(shader);
        bundlesToBuild[bundleName] = shaders;

        BuildBundles(bundlesToBuild);

        EditorUtility.DisplayDialog("Bundling Shaders", "The CYF Shader Bundle \"" + bundleName + "\" has been created!\n\nYou can find it in:\n" + assetBundleDirectory + "/" + bundleName, "OK");
    }

    private static Dictionary<string, List<Shader>> RetrieveAllBundles() {
        if (!Directory.Exists(shaderDirectory)) {
            Debug.LogError("The shaders directory (\"" + shaderDirectory + "\") does not exist. Thus, there are no bundles to build. Aborting.");
            return null;
        }

        // Get all assets
        string[] assets = Directory.GetFiles(shaderDirectory, "*.shader");
        Dictionary<string, List<Shader>> bundles = new Dictionary<string, List<Shader>>();

        // Get asset bundle names from each file
        foreach (string file in assets) {
            ShaderImporter importer = (ShaderImporter)AssetImporter.GetAtPath(file);

            if (importer == null) {
                Debug.LogWarning("Could not import asset \"" + file + "\". Skipping.");
                continue;
            }

            // Get asset bundle name
            string bundleName = importer.assetBundleName;
            if (bundleName != "") {
                // Create a folder for each bundle if applicable
                if (!Directory.Exists(assetBundleDirectory + "/" + bundleName))
                    Directory.CreateDirectory(assetBundleDirectory + "/" + bundleName);

                // Create a bundle if applicable
                if (!bundles.ContainsKey(bundleName)) {
                    List<Shader> list = new List<Shader>();
                    bundles[bundleName] = list;
                }
                bundles[bundleName].Add(importer.GetShader());
            }
        }

        if (bundles.Count == 0) {
            Debug.LogError("There are no AssetBundles to build. Aborting.");
            return null;
        }

        return bundles;
    }

    static void BuildBundles(Dictionary<string, List<Shader>> bundles) {
        if (!Directory.Exists(assetBundleDirectory))
            Directory.CreateDirectory(assetBundleDirectory);

        // Build AssetBundles
        foreach (KeyValuePair<string, List<Shader>> pair in bundles) {
            // Windows
            if (!BuildPipeline.BuildAssetBundle(null, pair.Value.ToArray(), assetBundleDirectory + "/" + pair.Key + "/windows", BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows))
                Debug.LogError("An error occured while building the AssetBundle \"" + pair.Key + "\".");

            // Linux
            if (!BuildPipeline.BuildAssetBundle(null, pair.Value.ToArray(), assetBundleDirectory + "/" + pair.Key + "/linux", BuildAssetBundleOptions.None, BuildTarget.StandaloneLinuxUniversal))
                Debug.LogError("An error occured while building the AssetBundle \"" + pair.Key + "\".");

            // Mac
            if (!BuildPipeline.BuildAssetBundle(null, pair.Value.ToArray(), assetBundleDirectory + "/" + pair.Key + "/mac", BuildAssetBundleOptions.None, BuildTarget.StandaloneOSX))
                Debug.LogError("An error occured while building the AssetBundle \"" + pair.Key + "\".");
            
        }
    }
}

public class BundleShaderDialog : EditorWindow {
    public string bundleName;

    void OnEnable() {
        this.titleContent = new GUIContent("CYF Shaders", "Utilities used to build CYF Shader AssetBundles for use with Create Your Frisk mods.");
    }

    void OnGUI() {
        bundleName = EditorGUILayout.TextField(new GUIContent("AssetBundle to build", "The name of a CYF Shader AssetBundle within the shaders directory to build."), bundleName);

        if (GUILayout.Button(new GUIContent("Build one AssetBundle", "Builds a CYF Shader AssetBundle with the name given in the text box above."))) {
            BundleShaders.OneBundle(bundleName);
            GUIUtility.ExitGUI();
        } else if (GUILayout.Button(new GUIContent("Build all AssetBundles", "Builds all CYF Shader AssetBundles set up in the shaders directory."))) {
            BundleShaders.AllBundlesOption();
            GUIUtility.ExitGUI();
        }
    }
}