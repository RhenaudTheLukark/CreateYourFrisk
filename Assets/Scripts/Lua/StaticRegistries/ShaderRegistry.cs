using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking; // See the comments above retrieveAssetBundle

public static class ShaderRegistry {
    public static Material UI_DEFAULT_MATERIAL;
    private static readonly Dictionary<string, Material> materialsDefault = new Dictionary<string, Material>();
    private static readonly Dictionary<string, Material> materialsMod     = new Dictionary<string, Material>();
    private static readonly Dictionary<string, AssetBundle> dictDefault = new Dictionary<string, AssetBundle>();
    private static readonly Dictionary<string, AssetBundle> dictMod     = new Dictionary<string, AssetBundle>();
    private static readonly string OSType = Misc.OSType.ToLower();

    // Load all default AssetBundles into memory one time, and never unload them
    // (except when the overworld gets restarted or whatnot)
    public static void Start() {
        UI_DEFAULT_MATERIAL = new Material(Shader.Find("UI/Default"));
        loadAllFrom(FileLoader.pathToDefaultFile("Shaders"), false);
    }

    // Retrieve a shader from a loaded AssetBundle
    public static Material Get(string bundle, string key) {
        string bundleL = bundle.ToLower();
        string keyL = key.ToLower();
        if (dictMod.ContainsKey(bundleL))
            return materialsMod.ContainsKey(bundleL + keyL) ? materialsMod[bundleL + keyL] : tryLoad(bundleL, keyL, true, bundle, key);

        if (dictDefault.ContainsKey(bundleL))
            return materialsDefault.ContainsKey(bundleL + keyL) ? materialsDefault[bundleL + keyL] : tryLoad(bundleL, keyL, false, bundle, key);
        throw new CYFException("Shader AssetBundle \"" + bundle + "\" could not be found in a mod or default directory.");
    }

    // Creates a new Material with an extracted shader
    private static Material tryLoad(string bundleLower, string keyLower, bool mod, string bundleName, string key) {
        AssetBundle bundle = mod ? dictMod[bundleLower] : dictDefault[bundleLower];

        if (!bundle.Contains(keyLower))
            throw new CYFException("Shader AssetBundle \"" + bundleName + "\" does not contain shader \"" + key + "\".");

        Shader shade = bundle.LoadAsset(keyLower) as Shader;
        if (shade != null && !shade.isSupported)
            throw new CYFException("The shader \"" + key + "\" is not supported. It might not be suited for your OS, or this might be a problem with the shader itself.");

        Material mat = new Material(shade);
        if (mod)    materialsMod[bundleLower + keyLower]     = mat;
        else        materialsDefault[bundleLower + keyLower] = mat;
        return mat;
    }

    // Unloads previous mod-specific Materials and opens AssetBundles for the new mod if applicable
    public static void Init() {
        materialsMod.Clear();
        loadAllFrom(FileLoader.pathToModFile("Shaders"), true);
        if (Camera.main.GetComponent<CameraShader>() && CameraShader.luashader != null)
            CameraShader.luashader.Revert();
    }

    // Opens all AssetBundles in a directory and stores them
    private static void loadAllFrom(string directoryPath, bool mod) {
        DirectoryInfo shaderFolder = new DirectoryInfo(directoryPath);

        if (!shaderFolder.Exists)
            return;

        DirectoryInfo[] shaderFolderContents = shaderFolder.GetDirectories("*", SearchOption.TopDirectoryOnly);

        List<FileInfo> shaderBundles = shaderFolderContents.Select(d => d.GetFiles("*.*", SearchOption.AllDirectories).Where(file => Path.GetExtension(file.Name) == "").ToArray()).Select(localShaders => localShaders.Single(s => s.Name == OSType)).Where(OSSpecificBundle => OSSpecificBundle != null).ToList();

        if (mod) {
            foreach (KeyValuePair<string, AssetBundle> pair in dictMod)
                try { pair.Value.Unload(true); }
                catch { /* ignored */ }

            dictMod.Clear();
            foreach (FileInfo file in shaderBundles) {
                DirectoryInfo parent = file.Directory;
                if (parent != null) dictMod[parent.Name] = retrieveAssetBundle(file.FullName);
            }
        } else {
            foreach (KeyValuePair<string, AssetBundle> pair in dictDefault)
                try { pair.Value.Unload(true); }
                catch { /* ignored */ }

            dictDefault.Clear();
            foreach (FileInfo file in shaderBundles) {
                DirectoryInfo parent = file.Directory;
                if (parent == null) continue;
                string bundle  = parent.Name;
                string bundleL = bundle.ToLower();
                dictDefault[bundleL] = retrieveAssetBundle(file.FullName);

                // Fill up dict with Default Materials
                string[] names = dictDefault[bundleL].GetAllAssetNames();
                // GetAllAssetNames returns "Assets/Editor/Shaders/myShader.shader" instead of just "myShader"
                // The default shaders have no subfolders, so we can safely just trim the string to everything after the last slash, and remove the ".shader"
                foreach (string key in names) {
                    string[] bits   = key.Split('/');
                    string   newKey = bits[bits.Length - 1].Replace(".shader", "");
                    string   keyL   = newKey.ToLower();
                    materialsDefault[bundleL + newKey] = tryLoad(bundleL, keyL, false, bundle, newKey);
                }
            }
        }
    }

    // NOTE: According to this guide (https://blogs.unity3d.com/2020/04/09/learn-to-save-memory-usage-by-improving-the-way-you-use-assetbundles/)
    // using AssetBundle.LoadFromFile (and LoadFromFileAsync) is only recommended when you intend to take out a lot of assets at once, or continually.
    // That is very not the goal for shaders in CYF, especially not Default shaders.
    // As such, instead of using AssetBundle.LoadFromFile, I'm using the page's recommended alternative, UnityWebRequestAssetBundle.GetAssetBundle.
    // If you wish to use the AssetBundle.LoadFromFile approach instead, uncomment the first line and comment the rest.
    private static AssetBundle retrieveAssetBundle(string fullPath) {
        //return AssetBundle.LoadFromFile(fullPath);

        try {
            UnityWebRequest uwr = UnityWebRequestAssetBundle.GetAssetBundle(new Uri(fullPath).AbsoluteUri.Replace("+", "%2B"));
            uwr.SendWebRequest();
            while (!uwr.isDone) { } // hold up a bit while it's loading; delay isn't noticeable and loading will fail otherwise
            AssetBundle content = DownloadHandlerAssetBundle.GetContent(uwr);
            if (content == null)   throw new CYFException("Error while loading the shader \"" + fullPath + "\".\n\nIt's likely that you have two identical shader AssetBundles loaded at once.");
            if (uwr.error != null) throw new CYFException(uwr.error);
            return content;
        } catch (CYFException e) {
            UnitaleUtil.DisplayLuaError("loading a shader", e.Message);
        } catch (Exception e) {
            UnitaleUtil.DisplayLuaError("loading a shader", "This is a " + e.GetType() + " error. Please show this screen to a developer.\n\n" + e.Message + "\n\n" + e.StackTrace);
        }
        return null;
    }
}