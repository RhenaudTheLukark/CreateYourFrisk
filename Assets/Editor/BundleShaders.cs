/// Some code from https://bitbucket.org/Unity-Technologies/assetbundledemo/src/default/
#pragma warning disable 0618
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;

public static class CreateAssetBundles {
    private static string assetBundleDirectory = "Assets/Editor/Output";
    private static string shaderDirectory = "Assets/Editor/Shaders";

    [MenuItem("Assets/Bundle CYF Shaders...")]
    static void BuildAllAssetBundles() {
        if (!Directory.Exists(assetBundleDirectory))
            Directory.CreateDirectory(assetBundleDirectory);

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

        EditorUtility.DisplayDialog("Bundling Shaders", "All CYF Shaders have been bundled!\n\nYou can find them in:\n" + assetBundleDirectory, "OK");
    }
}