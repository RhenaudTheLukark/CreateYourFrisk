using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MoonSharp.Interpreter.CoreLib;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Static utility class to take care of various file loading features in Unitale.
/// </summary>
public static class FileLoader {
    public static void calcDataRoot() {
        DirectoryInfo rootInfo = new DirectoryInfo(Application.dataPath);

        // Mac compatibility
        if (Application.platform == RuntimePlatform.OSXPlayer)
            rootInfo = rootInfo.Parent;

        if (rootInfo == null) return;
        string SysDepDataRoot = rootInfo.FullName;

        while (true) {
            DirectoryInfo[] dfs = rootInfo.GetDirectories();

            if (dfs.Any(df => df.FullName == Path.Combine(SysDepDataRoot, "Mods"))) {
                DataRoot = SysDepDataRoot.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
                return;
            }

            try {
                System.Diagnostics.Debug.Assert(rootInfo.Parent != null, "rootInfo.Parent != null");
                rootInfo = new DirectoryInfo(rootInfo.Parent.FullName);
            } catch {
                UnitaleUtil.DisplayLuaError("CYF's Startup", "The engine detected no Mods folder in your files: are you sure it exists?");
                return;
            }
            SysDepDataRoot = rootInfo.FullName;
            //Debug.Log(SysDepDataRoot);
        }

        //if (Application.platform == RuntimePlatform.OSXPlayer) /*OSX has stuff bundled in .app things*/                      SysDepDataRoot = rootInfo.Parent.Parent.FullName;
        //else if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor) { /*everything is fine*/ }
        //else                                                                                                                 SysDepDataRoot = rootInfo.Parent.FullName;
    }

    private static string _DataRoot;
    /// <summary>
    /// Get the full platform-dependent path to the application root (the folder in which the Unitale executable resides).
    /// </summary>
    public static string DataRoot {
        get {
            if (_DataRoot == null)
                calcDataRoot();
            return _DataRoot;
        }
        private set {
            _DataRoot = value;
            LoadModule.DataRoot = value;
        }
    }

    /// <summary>
    /// Get the full path to the main directory of the currently selected mod.
    /// </summary>
    public static string ModDataPath {
        get { return Path.Combine(DataRoot, "Mods/" + StaticInits.MODFOLDER); }
    }

    /// <summary>
    /// Get the path to the default Undertale assets directory.
    /// </summary>
    public static string DefaultDataPath {
        get { return Path.Combine(DataRoot, "Default"); }
    }

    /// <summary>
    /// Return the given file as a byte array.
    /// </summary>
    /// <param name="filename">Path to file that should be read</param>
    /// <param name="pathSuffix">Suffix of the file after the mod or default folder</param>
    /// <returns>Byte array containing all bytes in the file.</returns>
    public static byte[] GetBytesFrom(ref string filename, string pathSuffix = "Sprites/") {
        SanitizePath(ref filename, pathSuffix, true, true);
        return File.ReadAllBytes(filename);
    }

    /// <summary>
    /// Returns the path to the given file within the selected mod directory.
    /// </summary>
    /// <param name="filename">Path to file relative to mod directory root</param>
    /// <returns>Full path to the file specified</returns>
    public static string PathToModFile(string filename) { return Path.Combine(ModDataPath, filename); }

    /// <summary>
    /// Returns the path to the given file within the default directory.
    /// </summary>
    /// <param name="filename">Path to file relative to default directory root</param>
    /// <returns>Full path to the file specified</returns>
    public static string PathToDefaultFile(string filename) { return Path.Combine(DefaultDataPath, filename); }

    public static bool SceneExists(string name) {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            if (Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(i)) == name)
                return true;
        return false;
    }

    ///////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////

    /// <summary>
    /// Checks if a file exists in CYF's Default or Mods folder and returns a clean path to it without its extension using SanitizePath().
    /// </summary>
    /// <param name="fileName">Path to the file to require, relative or absolute. Will also contain the clean path to the existing resource if found.</param>
    /// <param name="pathSuffix">String to add to the tested path to check in the given folder.</param>
    /// <param name="errorOnFailure">Defines whether the error screen should be displayed if the file isn't in either folder.</param>
    /// <param name="needsAbsolutePath">True if you want to get the absolute path to the file, false otherwise.</param>
    /// <returns>True if the sanitization was successful, false otherwise.</returns>
    public static bool GetRelativePathWithoutExtension(ref string fileName, string pathSuffix, bool errorOnFailure = true, bool needsAbsolutePath = false) {
        bool result = SanitizePath(ref fileName, pathSuffix, errorOnFailure, needsAbsolutePath);
        if (!result) return false;
        int extIndex = fileName.LastIndexOf('.');
        fileName = extIndex > 0 ? fileName.Substring(0, extIndex) : fileName;
        return true;
    }

    public static Dictionary<string, string> relativeSanitizationDictionary = new Dictionary<string, string>();
    public static Dictionary<string, string> absoluteSanitizationDictionary = new Dictionary<string, string>();

    /// <summary>
    /// Checks if a file exists in CYF's Default or Mods folder and returns a clean path to it.
    /// It only runs RequireFile() if it's truly useful, otherwise it just checks if the file at the given path exists.
    /// </summary>
    /// <param name="fileName">Path to the file to require, relative or absolute. Will also contain the clean path to the existing resource if found.</param>
    /// <param name="pathSuffix">String to add to the tested path to check in the given folder.</param>
    /// <param name="errorOnFailure">Defines whether the error screen should be displayed if the file isn't in either folder.</param>
    /// <param name="needsAbsolutePath">True if you want to get the absolute path to the file, false otherwise.</param>
    /// <returns>True if the sanitization was successful, false otherwise.</returns>
    public static bool SanitizePath(ref string fileName, string pathSuffix, bool errorOnFailure = true, bool needsAbsolutePath = false, bool needsToExist = true) {
        fileName = fileName.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);

        // Check if this same string has been passed to RequireFile before
        if (needsAbsolutePath && absoluteSanitizationDictionary.ContainsKey(fileName)) {
            Debug.Log("Fast sanitization for " + fileName + " (absolute)");
            fileName = absoluteSanitizationDictionary[fileName];
            return true;
        }
        if (!needsAbsolutePath && relativeSanitizationDictionary.ContainsKey(fileName)) {
            Debug.Log("Fast sanitization for " + fileName + " (relative)");
            fileName = relativeSanitizationDictionary[fileName];
            return true;
        }

        // Sanitize if path from CYF root, need to transform a relative path to an absolute path and vice-versa, or if there's an occurence of ..
        if (fileName.StartsWith(Path.DirectorySeparatorChar.ToString()) || fileName.Contains(DataRoot) ^ needsAbsolutePath || fileName.Contains(".." + Path.DirectorySeparatorChar)) {
            string original = fileName;
            bool res = LoadModule.RequireFile(ref fileName, pathSuffix, errorOnFailure, needsAbsolutePath, needsToExist);
            if (needsAbsolutePath) absoluteSanitizationDictionary.Add(original, fileName);
            else                   relativeSanitizationDictionary.Add(original, fileName);
            return res;
        }

        if (fileName.Contains(DataRoot))
            return !needsToExist || new FileInfo(fileName).Exists;

        return !needsToExist || new FileInfo(PathToModFile(fileName)).Exists || new FileInfo(PathToDefaultFile(fileName)).Exists;
    }
}