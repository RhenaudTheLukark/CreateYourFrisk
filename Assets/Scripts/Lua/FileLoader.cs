using System;
using System.IO;
using System.Linq;
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

        string SysDepDataRoot = rootInfo.FullName;

        while (true) {
            DirectoryInfo[] dfs = rootInfo.GetDirectories();

            foreach (DirectoryInfo df in dfs)
                if (df.FullName == Path.Combine(SysDepDataRoot, "Mods")) {
                    DataRoot = SysDepDataRoot;
                    return;
                }

            try { rootInfo = new DirectoryInfo(rootInfo.Parent.FullName); }
            catch {
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

    private static string _DataRoot = null;
    /// <summary>
    /// Get the full platform-dependent path to the application root (the folder in which the Unitale executable resides).
    /// </summary>
    public static string DataRoot {
        get {
            if (_DataRoot == null)
                calcDataRoot();
            return _DataRoot;
        }
        private set { _DataRoot = value; }
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
    /// Return all text from a file as a string.
    /// </summary>
    /// <param name="filename">Path to file that should be read</param>
    /// <returns>String containing all text in the file.</returns>
    public static string getTextFrom(string filename) { return File.ReadAllText(requireFile(filename)); }

    /// <summary>
    /// Return the given file as a byte array.
    /// </summary>
    /// <param name="filename">Path to file that should be read</param>
    /// <returns>Byte array containing all bytes in the file.</returns>
    public static byte[] getBytesFrom(string filename) { return File.ReadAllBytes(requireFile(filename)); }

    /// <summary>
    /// Returns the path to the given file within the selected mod directory.
    /// </summary>
    /// <param name="filename">Path to file relative to mod directory root</param>
    /// <returns>Full path to the file specified</returns>
    public static string pathToModFile(string filename) { return Path.Combine(ModDataPath, filename); }

    /// <summary>
    /// Returns the path to the given file within the default directory.
    /// </summary>
    /// <param name="filename">Path to file relative to default directory root</param>
    /// <returns>Full path to the file specified</returns>
    public static string pathToDefaultFile(string filename) { return Path.Combine(DefaultDataPath, filename); }

    /// <summary>
    /// Returns the path to the given file in the mod folder if it exists, otherwise the default folder. If it doesn't exist, returns null and optionally shows the error screen.
    /// </summary>
    /// <param name="filename">Filename to require, relative to either mod or default folder root</param>
    /// <param name="errorOnFailure">Defines whether the error screen should be displayed if the file isn't in either folder.</param>
    /// <returns>File path if it exists, null otherwise (closely followed by error screen)</returns>
    public static string requireFile(string filename, bool errorOnFailure = true) {
        FileInfo fi = new FileInfo(pathToModFile(filename));
        if (!fi.Exists)
            fi = new FileInfo(pathToDefaultFile(filename));

        if (!fi.Exists) {
            if (errorOnFailure)
                if (filename.Length != 0)
                    throw new CYFException("Attempted to load " + filename + " from either a mod or default directory, but it was missing in both.");
                    //UnitaleUtil.DisplayLuaError("???", "Attempted to load " + filename + " from either a mod or default directory, but it was missing in both.");
            return null;
        }

        return fi.FullName;
    }

    public static bool SceneExists(string name) {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            if (Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(i)) == name)
                return true;
        return false;
    }



    public static AudioClip getAudioClipFromMod(string relPath, string clipName)
        { return getAudioClip(pathToModFile(relPath), pathToModFile(relPath + Path.DirectorySeparatorChar + clipName)); }

    public static AudioClip getAudioClipFromDefault(string relPath, string clipName)
        { return getAudioClip(pathToDefaultFile(relPath), pathToDefaultFile(relPath + Path.DirectorySeparatorChar + clipName)); }

    /// <summary>
    /// Get an AudioClip at the given full path. Attempts to retrieve it from the AudioClipRegistry first by using folderRoot to extract the clip's name, otherwise attempts to load from disk.
    /// </summary>
    /// <param name="musicFilePath">Full path to a file.</param>
    /// <returns>AudioClip object on successful load, otherwise null.</returns>
    public static AudioClip getAudioClip(string folderRoot, string musicFilePath) {
        string clipName = FileLoader.getRelativePathWithoutExtension(folderRoot, musicFilePath);
        //AudioClip music = AudioClipRegistry.Get(clipName);
        //if (music != null)
        //    return music;
        WWW www = new WWW(new Uri(musicFilePath).AbsoluteUri.Replace("+", "%2B"));
        while (!www.isDone) { } // hold up a bit while it's loading; delay isn't noticeable and loading will fail otherwise
        AudioType type = AudioType.UNKNOWN;
        if (musicFilePath.EndsWith(".ogg"))       type = AudioType.OGGVORBIS;
        else if (musicFilePath.EndsWith(".wav"))  type = AudioType.WAV;
        else                                      return null;

        AudioClip music;
        music = www.GetAudioClip(false, false, type);
        music.name = "File at " + musicFilePath;
        music.LoadAudioData();

        AudioClipRegistry.Set(clipName, music);
        return music;
    }

    public static string getRelativePathWithoutExtension(string fullPath) {
        fullPath = fullPath.Replace('\\', '/');
        if (fullPath.Contains(ModDataPath.Replace('\\', '/')))
            fullPath = fullPath.Substring(ModDataPath.Length + 9, fullPath.Length - ModDataPath.Length - 13);
        if (fullPath.Contains(DefaultDataPath.Replace('\\', '/')))
            fullPath = fullPath.Substring(DefaultDataPath.Length + 9, fullPath.Length - DefaultDataPath.Length - 13);
        return fullPath;
    }

    public static string getRelativePathWithoutExtension(string rootPath, string fullPath) {
        if (rootPath[rootPath.Length-1] != Path.DirectorySeparatorChar)
            rootPath += Path.DirectorySeparatorChar;
        Uri uriFull = new Uri(fullPath),
            uriRel = new Uri(rootPath).MakeRelativeUri(uriFull);
        string relativePath = Uri.UnescapeDataString(uriRel.OriginalString);
        int extIndex = relativePath.LastIndexOf('.');
        if (extIndex > 0)  return relativePath.Substring(0, extIndex);
        else               return relativePath;
    }
}