using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

public class KeyboardInput : IUndertaleInput {
    /// <summary>
    /// List of axes handled by CYF.
    /// </summary>
    public static readonly Dictionary<string, float> axes = new Dictionary<string, float>();
    /// <summary>
    /// List of known axes keys, for quick computation.
    /// </summary>
    private static List<string> knownAxes = new List<string>();
    /// <summary>
    /// Number of controllers handled by CYF.
    /// Add more inputs in CYF's Input settings if you wanna handle more controllers!
    /// </summary>
    private const int controllers = 2;
    /// <summary>
    /// Number of axes handled by CYF.
    /// Add more inputs in CYF's Input settings if you wanna handle more axes!
    /// </summary>
    private const int axesNumber = 10;

    /// <summary>
    /// Dictionary storing the various default keybindings of Create Your Frisk.
    /// </summary>
    public readonly static Dictionary<string, List<string>> defaultKeys = new Dictionary<string, List<string>>() {
        { "Confirm", new List<string> { "Z", "Return" } },
        { "Cancel", new List<string> { "X", "LeftShift", "RightShift" } },
        { "Menu", new List<string> { "C", "LeftControl", "RightControl" } },
        { "Up", new List<string> { "W", "UpArrow", "Vertical1 +" } },
        { "Down", new List<string> { "S", "DownArrow", "Vertical1 -" } },
        { "Left", new List<string> { "A", "LeftArrow", "Horizontal1 -" } },
        { "Right", new List<string> { "D", "RightArrow", "Horizontal1 +" } },
    };
    /// <summary>
    /// Dictionary storing the various keybindings set by the user.
    /// Can be modified through Create Your Frisk's Options menu.
    /// </summary>
    public static Dictionary<string, List<string>> playerKeys = new Dictionary<string, List<string>>();
    /// <summary>
    /// Dictionary storing the various keybindings in effect during the current encounter.
    /// Can be modified through various Input functions.
    /// </summary>
    public static Dictionary<string, List<string>> encounterKeys = new Dictionary<string, List<string>>();

    /// <summary>
    /// This function is executed whenever this object is created.
    /// </summary>
    public KeyboardInput() {
        foreach (KeyValuePair<string, List<string>> keybind in defaultKeys) {
            playerKeys[keybind.Key] = new List<string>(keybind.Value);
            encounterKeys[keybind.Key] = new List<string>(keybind.Value);
        }

        for (int controller = 1; controller <= controllers; controller++) {
            for (int axis = 1; axis <= axesNumber; axis++) {
                string axisName;
                if (axis == 1)      axisName = "Horizontal";
                else if (axis == 2) axisName = "Vertical";
                else                axisName = "Axis" + axis + "-";
                axisName += controller;

                knownAxes.Add(axisName + " +");
                knownAxes.Add(axisName + " -");
                axes[axisName] = 0;
            }
        }
    }

    /// <summary>
    /// This function resets the user's keybindings after a battle, in case they were tampered with during it.
    /// </summary>
    public static void ResetEncounterInputs() {
        encounterKeys.Clear();
        foreach (KeyValuePair<string, List<string>> keybind in playerKeys)
            encounterKeys[keybind.Key] = new List<string>(keybind.Value);
    }
    /// <summary>
    /// This function resets the user's keybindings, it should only be used when the user asks to reset all keybindings to their default.
    /// </summary>
    public static void ResetInputs() {
        playerKeys.Clear();
        foreach (KeyValuePair<string, List<string>> keybind in defaultKeys)
            playerKeys[keybind.Key] = new List<string>(keybind.Value);
        ResetEncounterInputs();
    }
    /// <summary>
    /// This function reset the user's keybindings on a given key, leaving any other key untouched.
    /// </summary>
    /// <param name="keybind">The key to reset the bindings of.</param>
    public static void ResetSpecificInput(string keybind) {
        if (defaultKeys[keybind] == null)
            throw new CYFException("CYF doesn't know the default keybind \"" + keybind + "\". Please refer to the list of known default keybinds in the Input object page of the documentation.");
        playerKeys[keybind] = new List<string>(defaultKeys[keybind]);
    }

    /// <summary>
    /// This function creates a new keybind for the encounter and assigns it some keys if they're given.
    /// </summary>
    /// <param name="keybind">Name of the new keybind to create.</param>
    /// <param name="keysToBind">Keys assigned to the keybind.</param>
    public static void CreateKeybind(string keybind, string[] keysToBind = null) {
        if (encounterKeys.ContainsKey(keybind))
            throw new CYFException("The keybind \"" + keybind + "\" already exists yet you tried creating it again.");

        List<string> keyCodesToBind = new List<string>();
        if (keysToBind != null)
            foreach (string key in keysToBind)
                keyCodesToBind.Add(key);

        encounterKeys.Add(keybind, keyCodesToBind);
    }

    /// <summary>
    /// This function completely deletes a keybind from the doctionary of keybinds.
    /// Note that base CYF keybinds cannot be deleted, as it would cause errors when the engine tries to fetch them.
    /// </summary>
    /// <param name="keybind">The keybind to delete.</param>
    public static void DeleteKeybind(string keybind) {
        if (defaultKeys.ContainsKey(keybind))
            throw new CYFException("CYF's base keybinds cannot be deleted!");
        encounterKeys.Remove(keybind);
    }

    /// <summary>
    /// This function returns an array of all the keys bound to a given keybind.
    /// </summary>
    /// <param name="keybind">The keybind to get the keys of.</param>
    /// <returns>The various eys bound to the given keybind.</returns>
    public static string[] GetKeybindKeys(string keybind) {
        if (!encounterKeys.ContainsKey(keybind))
            throw new CYFException("The keybind \"" + keybind + "\" doesn't exist.");
        return encounterKeys[keybind].Select(k => k.ToString()).ToArray();
    }

    /// <summary>
    /// This function returns the keys bound to several keybinds.
    /// </summary>
    /// <param name="keybinds">Keybind collection to check for conflicts.</param>
    /// <returns>Dictionary of conflicts found within the collection.</returns>
    public static Dictionary<string, string[]> GetConflicts(Dictionary<string, List<string>> keybinds) {
        Dictionary<string, string[]> conflicts = new Dictionary<string, string[]>();

        foreach (KeyValuePair<string, List<string>> keybind in keybinds)
            foreach (string key in keybind.Value) {
                List<string> linkedKeybinds = conflicts.ContainsKey(key) ? conflicts[key].ToList() : new List<string>();
                linkedKeybinds.Add(keybind.Key);
                conflicts[key] = linkedKeybinds.ToArray();
            }

        return conflicts.Where(p => p.Value.Length > 1).ToDictionary(p => p.Key, p => p.Value);
    }

    /// <summary>
    /// This function replaces all of a keybind's bound keys with another set of keys.
    /// </summary>
    /// <param name="keybind">Name of the keybind to replace the keys of.</param>
    /// <param name="keysToBind">List of new keys to assign to the keybind.</param>
    public static void SetKeybindKeys(string keybind, string[] keysToBind) {
        if (!encounterKeys.ContainsKey(keybind))
            throw new CYFException("The keybind \"" + keybind + "\" doesn't exist.");

        List<string> keyCodesToBind = new List<string>();
        if (keysToBind != null)
            foreach (string key in keysToBind) {
                if (!CheckKeyValidity(key))
                    throw new CYFException("The key \"" + key + "\" isn't recognized by CYF.");
                keyCodesToBind.Add(key);
            }

        encounterKeys[keybind] = keyCodesToBind;
    }

    /// <summary>
    /// This function adds a key to an existing keybind.
    /// </summary>
    /// <param name="keybind">Name of the keybind to add a key to.</param>
    /// <param name="key">Key to add to the keybind.</param>
    public static bool AddKeyToKeybind(string keybind, string key) {
        List<string> keys;
        encounterKeys.TryGetValue(keybind, out keys);
        if (keys == null)
            throw new CYFException("The keybind \"" + keybind + "\" doesn't exist.");

        if (!CheckKeyValidity(key))
            throw new CYFException("The key \"" + key + "\" isn't recognized by CYF.");

        if (!keys.Contains(key)) {
            keys.Add(key);
            encounterKeys[keybind] = keys;
            return true;
        }
        return false;
    }
    /// <summary>
    /// This function removes a key from an existing keybind.
    /// </summary>
    /// <param name="keybind">Name of the keybind to remove a key from.</param>
    /// <param name="key">Key to remove from the keybind.</param>
    public static bool RemoveKeyFromKeybind(string keybind, string key) {
        List<string> keys;
        encounterKeys.TryGetValue(keybind, out keys);
        if (keys == null)
            throw new CYFException("The keybind \"" + keybind + "\" doesn't exist.");

        if (!CheckKeyValidity(key))
            throw new CYFException("The key \"" + key + "\" isn't recognized by CYF.");

        if (keys.Contains(key)) {
            keys.Remove(key);
            encounterKeys[keybind] = keys;
            return true;
        }
        return false;
    }

    /// <summary>
    /// This function returns true if the keybind exists, false otherwise.
    /// </summary>
    /// <param name="keybind">The keybind to check the existence of.</param>
    /// <returns>True if the keybind exists, false otherwise.</returns>
    public static bool KeybindExists(string keybind) {
        return encounterKeys.ContainsKey(keybind);
    }

    /// <summary>
    /// This function loads the player's keybinding configuration stored in their AlMightyGlobals.
    /// </summary>
    public static void LoadPlayerKeys() {
        if (!File.Exists(Application.persistentDataPath + "/keybinds.gd"))
            return;

        UTF8Encoding utf8 = new UTF8Encoding();
        string fileContents = "";
        using (FileStream file = File.OpenRead(Application.persistentDataPath + "/keybinds.gd")) {
            byte[] buffer = new byte[1024];
            int offset = 0;
            while (offset < file.Length - 1) {
                offset += file.Read(buffer, offset, 1024);
                fileContents += utf8.GetString(buffer);
            }
            file.Dispose();
        }

        try {
            foreach (string keybind in fileContents.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)) {
                string[] keyValue = keybind.Split(':');
                if (keyValue.Length != 2)
                    throw new Exception("The keybind format of the line " + keybind + " doesn't follow CYF's standard keybind format.");

                List<string> keys = keyValue[1].Split('|').Select(k => k.TrimEnd('\0')).ToList();
                foreach (string key in keys)
                    if (!CheckKeyValidity(key))
                        throw new CYFException("The key \"" + key + "\" isn't recognized by CYF for the keybind \"" + keyValue[0] + "\".");

                playerKeys[keyValue[0]] = keys;
            }
        } catch (Exception e) {
            UnitaleUtil.DisplayLuaError("keybind loading", "Error while loading the user's keybind configuration.\nPlease delete the file named \"keybinds.gd\" in CYF's save folder, in this path:\n\n<b>" + Application.persistentDataPath + "/keybinds.gd</b>\n\nActual error:\n" + e.Message, true);
        }
        ResetEncounterInputs();
    }
    /// <summary>
    /// This function saves the player's keybinding configuration into their AlMightyGlobals.
    /// </summary>
    public static void SaveKeybinds(Dictionary<string, List<string>> newKeys) {
        if (File.Exists(Application.persistentDataPath + "/keybinds.gd"))
            File.Delete(Application.persistentDataPath + "/keybinds.gd");

        UTF8Encoding utf8 = new UTF8Encoding();
        string debugFile = "";
        using (FileStream file = File.OpenWrite(Application.persistentDataPath + "/keybinds.gd")) {
            int fileLength = 0;
            foreach (KeyValuePair<string, List<string>> keybind in newKeys) {
                string keybindString = (fileLength > 0 ? Environment.NewLine : "") + keybind.Key + ":" + string.Join("|", keybind.Value.OrderBy(k => k.Length).ToArray());
                debugFile += keybindString;
                file.Write(utf8.GetBytes(keybindString), 0, keybindString.Length);
                file.Flush();
                fileLength += keybindString.Length;
            }
            file.Dispose();
        }
        playerKeys.Clear();
        foreach (KeyValuePair<string, List<string>> keybind in newKeys)
            playerKeys[keybind.Key] = new List<string>(keybind.Value);
        ResetEncounterInputs();
    }

    /// <summary>
    /// This function returns whether a given key is recognized by CYF or not.
    /// </summary>
    /// <param name="key">KEy to check for.</param>
    /// <returns>True if the key is valid, false otherwise.</returns>
    public static bool CheckKeyValidity(string key) {
        KeyCode keycode;
        if (ParseUtil.TryParseEnum(typeof(KeyCode), key, out keycode))
            return true;

        if (knownAxes.Contains(key))
            return true;

        return false;
    }

    // Shortcuts for existing keys
    public ButtonState Confirm { get { return StateFor("Confirm"); } }
    public ButtonState Cancel { get { return StateFor("Cancel"); } }
    public ButtonState Menu { get { return StateFor("Menu"); } }
    public ButtonState Up { get { return StateFor("Up"); } }
    public ButtonState Down { get { return StateFor("Down"); } }
    public ButtonState Left { get { return StateFor("Left"); } }
    public ButtonState Right { get { return StateFor("Right"); } }

    /// <summary>
    /// This function queries the current state of an existing keyboard key or axis.
    /// </summary>
    /// <param name="Key">A keyboard key known by Unity, or axis.</param>
    /// <returns>ButtonState depending on the state of the key (pressed, held, released, none).</returns>
    public ButtonState Key(string key) {
        if (knownAxes.Contains(key))
            return StateForAxis(key);
        return StateFor((KeyCode)Enum.Parse(typeof(KeyCode), key));
    }

    /// <summary>
    /// This function returns the state of the selected key.
    /// </summary>
    /// <param name="c">Key to check the state of.</param>
    /// <returns>state of the key (pressed, held, released, none).</returns>
    private static ButtonState StateFor(KeyCode c) {
        if (Input.GetKeyDown(c)) return ButtonState.PRESSED;
        if (Input.GetKeyUp(c))   return ButtonState.RELEASED;
        return Input.GetKey(c) ? ButtonState.HELD
                               : ButtonState.NONE;
    }

    /// <summary>
    /// Priority of each key state, in descending order:
    /// 2 = HELD
    /// 1 = PRESSED
    /// -1 = RELEASED
    /// 0 = NONE
    /// </summary>
    private static readonly int[] priority = new int[] { 2, 1, -1, 0 };
    /// <summary>
    /// This function returns the state with the highest priority among the keys linked to a named key.
    /// </summary>
    /// <param name="keybind">Named key to check for (ex: Confirm).</param>
    /// <returns>State of the key with the highest priority among the set.</returns>
    public static ButtonState StateFor(string keybind) {
        List<string> keys;
        encounterKeys.TryGetValue(keybind, out keys);
        if (keys == null)
            throw new CYFException("The keybind or key \"" + keybind + "\" doesn't exist.");

        List<KeyCode> keycodes = new List<KeyCode>();
        List<string> axes = new List<string>();

        foreach (string key in keys) {
            if (knownAxes.Contains(key)) axes.Add(key);
            else                         keycodes.Add((KeyCode)Enum.Parse(typeof(KeyCode), key));
        }

        List<ButtonState> states = keycodes.Select(k => StateFor(k)).ToList();
        foreach (string axis in axes)
            states.Add(StateForAxis(axis));

        ButtonState result = ButtonState.NONE;
        int resultPriority = Array.IndexOf(priority, (int)result);
        foreach (ButtonState state in states) {
            int statePriority = Array.IndexOf(priority, (int)state);
            if (statePriority < resultPriority) {
                result = state;
                resultPriority = statePriority;
                if (resultPriority == 0)
                    break;
            }
        }

        return result;
    }

    /// <summary>
    /// This function returns the state of the selected axis key.
    /// </summary>
    /// <param name="axisKey">Axis key to check for.</param>
    /// <returns>The state of the selected axis key</returns>
    public static ButtonState StateForAxis(string axisKey) {
        int state = 0;
        string axisName = axisKey.Substring(0, axisKey.Length - 2);
        float axisState = Input.GetAxis(axisName);
        if (axisKey.Substring(axisKey.Length - 1).Equals("+")) {
            if (axes[axisName] >= 0.7f)
                state -= 1;
            if (axisState > 0.7f)
                state = -state + 1;
        } else {
            if (axes[axisName] <= -0.7f)
                state -= 1;
            if (axisState < -0.7f)
                state = -state + 1;
        }
        return (ButtonState)state;
    }

    /// <summary>
    /// This function is run after all other Update() calls within the engine are run.
    /// </summary>
    public void LateUpdate() {
        string[] axesNames = axes.Keys.ToArray();
        foreach (string axis in axesNames)
            axes[axis] = Input.GetAxis(axis);
    }
}
