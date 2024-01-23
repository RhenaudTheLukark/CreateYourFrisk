using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class KeyboardInput : UndertaleInput {
    /// <summary>
    /// Dictionary storing the various default keybindings of Create Your Frisk.
    /// </summary>
    public readonly static Dictionary<string, List<KeyCode>> defaultKeys = new Dictionary<string, List<KeyCode>>() {
        { "Confirm", new List<KeyCode> { KeyCode.Z, KeyCode.Return } },
        { "Cancel", new List<KeyCode> { KeyCode.X, KeyCode.LeftShift, KeyCode.RightShift } },
        { "Menu", new List<KeyCode> { KeyCode.C, KeyCode.LeftControl, KeyCode.RightControl } },
        { "Up", new List<KeyCode> { KeyCode.W, KeyCode.UpArrow } },
        { "Down", new List<KeyCode> { KeyCode.S, KeyCode.DownArrow } },
        { "Left", new List<KeyCode> { KeyCode.A, KeyCode.LeftArrow } },
        { "Right", new List<KeyCode> { KeyCode.D, KeyCode.RightArrow } },
    };
    /// <summary>
    /// Dictionary storing the various keybindings set by the user.
    /// Can be modified through Create Your Frisk's Options menu.
    /// </summary>
    public static Dictionary<string, List<KeyCode>> generalKeys = new Dictionary<string, List<KeyCode>>(defaultKeys);
    /// <summary>
    /// Dictionary storing the various keybindings in effect during the current encounter.
    /// Can be modified through various Input functions.
    /// </summary>
    public static Dictionary<string, List<KeyCode>> encounterKeys = new Dictionary<string, List<KeyCode>>(generalKeys);

    /// <summary>
    /// This function is executed whenever this object is created.
    /// </summary>
    void Start() {
        LoadPlayerKeys();
    }

    /// <summary>
    /// This function resets the user's keybindings after a battle, in case they were tampered with during it.
    /// </summary>
    public static void ResetEncounterInputs() {
        encounterKeys = new Dictionary<string, List<KeyCode>>(generalKeys);
    }
    /// <summary>
    /// This function resets the user's keybindings, it should only be used when the user asks to reset all keybindings to their default.
    /// </summary>
    public static void ResetInputs() {
        generalKeys = new Dictionary<string, List<KeyCode>>(defaultKeys);
        ResetEncounterInputs();
    }
    /// <summary>
    /// This function reset the user's keybindings on a given key, leaving any other key untouched.
    /// </summary>
    /// <param name="keybind">The key to reset the bindings of.</param>
    public static void ResetSpecificInput(string keybind) {
        if (defaultKeys[keybind] == null)
            throw new CYFException("CYF doesn't know the default keybind \"" + keybind + "\". Please refer to the list of known default keybinds in the Input object page of the documentation.");
        generalKeys[keybind] = defaultKeys[keybind];
    }

    /// <summary>
    /// This function creates a new keybind for the encounter and assigns it some keys if they're given.
    /// </summary>
    /// <param name="keybind">Name of the new keybind to create.</param>
    /// <param name="keysToBind">Keys assigned to the keybind.</param>
    public static void CreateKeybind(string keybind, string[] keysToBind = null) {
        if (encounterKeys.ContainsKey(keybind))
            throw new CYFException("The keybind \"" + keybind + "\" already exists yet you tried creating it again.");

        List<KeyCode> keyCodesToBind = new List<KeyCode>();
        if (keysToBind != null)
            foreach (string key in keysToBind)
                keyCodesToBind.Add((KeyCode)Enum.Parse(typeof(KeyCode), key));

        encounterKeys.Add(keybind, keyCodesToBind);
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

    public static Dictionary<KeyCode, string[]> GetConflicts(Dictionary<string, List<KeyCode>> keybinds) {
        Dictionary<KeyCode, string[]> conflicts = new Dictionary<KeyCode, string[]>();

        foreach (KeyValuePair<string, List<KeyCode>> keybind in keybinds)
            foreach (KeyCode key in keybind.Value) {
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

        List<KeyCode> keyCodesToBind = new List<KeyCode>();
        if (keysToBind != null)
            foreach (string key in keysToBind)
                keyCodesToBind.Add((KeyCode)Enum.Parse(typeof(KeyCode), key));

        encounterKeys[keybind] = keyCodesToBind;
    }

    /// <summary>
    /// This function adds a key to an existing keybind.
    /// </summary>
    /// <param name="keybind">Name of the keybind to add a key to.</param>
    /// <param name="key">Key to add to the keybind.</param>
    public static bool AddKeyToKeybind(string keybind, string key) {
        List<KeyCode> keys;
        encounterKeys.TryGetValue(keybind, out keys);
        if (keys == null)
            throw new CYFException("The keybind \"" + keybind + "\" doesn't exist.");

        KeyCode keycode = (KeyCode)Enum.Parse(typeof(KeyCode), key);
        if (!keys.Contains(keycode)) {
            keys.Add(keycode);
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
        List<KeyCode> keys;
        encounterKeys.TryGetValue(keybind, out keys);
        if (keys == null)
            throw new CYFException("The keybind \"" + keybind + "\" doesn't exist.");

        KeyCode keycode = (KeyCode)Enum.Parse(typeof(KeyCode), key);
        if (keys.Contains(keycode)) {
            keys.Remove(keycode);
            encounterKeys[keybind] = keys;
            return true;
        }
        return false;
    }

    /// <summary>
    /// This function loads the player's keybinding configuration stored in their AlMightyGlobals.
    /// </summary>
    public static void LoadPlayerKeys() {
        Dictionary<string, List<KeyCode>> keys = new Dictionary<string, List<KeyCode>>(generalKeys);
        foreach (string key in keys.Keys) {
            DynValue keysString = LuaScriptBinder.GetAlMighty(null, "CYFKeybind" + key);
            if (keysString == null || keysString.Type != DataType.String || keysString.String == "")
                continue;
            List<KeyCode> keycodes = keysString.String.Split('|').Select(k => (KeyCode)Enum.Parse(typeof(KeyCode), k)).ToList();
            generalKeys[key] = keycodes;
        }
        ResetEncounterInputs();
    }
    /// <summary>
    /// This function loads the player's keybinding configuration stored in their AlMightyGlobals.
    /// </summary>
    public static void SaveKeybinds(Dictionary<string, List<KeyCode>> newKeys) {
        foreach (string key in newKeys.Keys) {
            List<KeyCode> keys = newKeys[key];
            string keysString = string.Join("|", keys.Select(k => k.ToString()).ToArray());
            LuaScriptBinder.SetAlMighty(null, "CYFKeybind" + key, DynValue.NewString(keysString));
        }
        generalKeys = newKeys;
        ResetEncounterInputs();
    }

    // Shortcuts for existing keys
    public override ButtonState Confirm { get { return StateFor("Confirm"); } }
    public override ButtonState Cancel { get { return StateFor("Cancel"); } }
    public override ButtonState Menu { get { return StateFor("Menu"); } }
    public override ButtonState Up { get { return StateFor("Up"); } }
    public override ButtonState Down { get { return StateFor("Down"); } }
    public override ButtonState Left { get { return StateFor("Left"); } }
    public override ButtonState Right { get { return StateFor("Right"); } }

    /// <summary>
    /// This function queries the current state of an existing keyboard key.
    /// </summary>
    /// <param name="Key">A keyboard key known by Unity.</param>
    /// <returns>ButtonState depending on the state of the key (pressed, held, released, none).</returns>
    public override ButtonState Key(string Key) { return StateFor((KeyCode)Enum.Parse(typeof(KeyCode), Key)); }

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
        List<KeyCode> keys;
        encounterKeys.TryGetValue(keybind, out keys);
        if (keys == null)
            throw new CYFException("The keybind \"" + keybind + "\" doesn't exist.");

        ButtonState result = ButtonState.NONE;
        int resultPriority = Array.IndexOf(priority, (int)result);
        ButtonState[] states = keys.Select(k => StateFor(k)).ToArray();
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
}
