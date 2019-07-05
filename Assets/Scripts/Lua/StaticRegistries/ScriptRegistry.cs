using System.Collections.Generic;
using System.IO;

public class ScriptRegistry {
    internal static string WAVE_PREFIX = "wave_";
    internal static string ENCOUNTER_PREFIX = "enc_";
    internal static string MONSTER_PREFIX = "mon_";
    internal static string EVENT_PREFIX = "event_";
    internal static string SHOP_PREFIX = "shop_";
    public static Dictionary<string, string> dict = new Dictionary<string, string>();

    private static string[] folders = new string[] { "Waves", "Encounters", "Monsters", "Events", "Shops" };
    private static string[] prefixes = new string[] { WAVE_PREFIX, ENCOUNTER_PREFIX, MONSTER_PREFIX, EVENT_PREFIX, SHOP_PREFIX };

    public static string Get(string key) {
        key = key.ToLower();
        if (dict.ContainsKey(key))
            return dict[key];
        return null;
    }

    public static void Set(string key, string value) { dict[key.ToLower()] = value; }

    public static void init() {
        dict.Clear();
        for (int i = 0; i < folders.Length; i++) {
            loadAllFrom(folders[i], prefixes[i], StaticInits.MODFOLDER != "@Title" && i == 1);
        }
    }

    private static void loadAllFrom(string folderName, string script_prefix, bool needed) {
        string directoryPath = FileLoader.pathToModFile("Lua/" + folderName);
        DirectoryInfo dInfo = new DirectoryInfo(directoryPath);
        if (!dInfo.Exists) {
            if (needed) {
                UnitaleUtil.DisplayLuaError("mod loading", "You tried to load the mod \"" + StaticInits.MODFOLDER + "\" but it can't be found, or at least its \"Lua/" + folderName + "\" folder can't be found.\nAre you sure it exists?");
                throw new CYFException("mod loading");
            }
            return;
        }
        FileInfo[] fInfo = dInfo.GetFiles("*.lua", SearchOption.AllDirectories);
        foreach (FileInfo file in fInfo) {
            //UnitaleUtil.writeInLog(file.Name);
            string scriptName = FileLoader.getRelativePathWithoutExtension(directoryPath, file.FullName).ToLower();
            string temp = "";
            dict.TryGetValue(script_prefix + scriptName, out temp);

            if (dict.ContainsKey(script_prefix + scriptName) && temp == FileLoader.getTextFrom(file.FullName))
                continue;

            else if (dict.ContainsKey(script_prefix + scriptName))
                dict.Remove(script_prefix + scriptName);

            Set(script_prefix + scriptName, FileLoader.getTextFrom(file.FullName));
        }
    }
}