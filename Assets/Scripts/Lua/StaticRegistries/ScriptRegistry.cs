using System.Collections.Generic;
using System.IO;

public class ScriptRegistry {
    public static Dictionary<string, string> dict = new Dictionary<string, string>();

    public static void Set(string key, string value) { dict[key.ToLower()] = value; }

    public static string Get(string key) {
        key += key.EndsWith(".lua") ? "" : ".lua";
        FileLoader.SanitizePath(ref key, "Lua/");
        key = key.ToLower();
        return dict.ContainsKey(key) ? dict[key] : null;
    }

    public static void Init() { LoadAllFrom(StaticInits.MODFOLDER != "@Title"); }

    private static void LoadAllFrom(bool needed) {
        string directoryPath = FileLoader.PathToModFile("Lua");
        DirectoryInfo dInfo = new DirectoryInfo(directoryPath);
        if (!dInfo.Exists) {
            if (!needed) return;
            UnitaleUtil.DisplayLuaError("mod loading", "You tried to load the mod \"" + StaticInits.MODFOLDER + "\" but it can't be found, or at least its Lua folder can't be found.\nAre you sure it exists?");
            throw new CYFException("mod loading");
        }

        FileInfo[] fInfo = dInfo.GetFiles("*.lua", SearchOption.AllDirectories);
        dict.Clear();
        foreach (FileInfo file in fInfo) {
            string k   = file.FullName.Substring(directoryPath.Length + 1),
                   val = k;
            FileLoader.SanitizePath(ref k, "Lua/");
            FileLoader.SanitizePath(ref val, "Lua/", true, true);
            Set(k.ToLower(), File.ReadAllText(val));
        }
    }
}