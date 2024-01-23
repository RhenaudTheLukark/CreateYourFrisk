using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class KeybindSettings : MonoBehaviour {
    public KeybindEntry Confirm, Cancel, Menu, Up, Left, Down, Right;
    public Text Listening;
    public Button Save, ResetAll, Restore, Back;

    private Dictionary<string, List<KeyCode>> tempKeybinds = new Dictionary<string, List<KeyCode>>(KeyboardInput.generalKeys);

    private CYFTimer textHijackTimer;
    private CYFTimer resetAllTimer;
    private CYFTimer restoreTimer;

    [HideInInspector] public KeybindEntry listening = null;

    void Start() {
        textHijackTimer = new CYFTimer(3, UpdateListeningText);
        resetAllTimer = new CYFTimer(3, CancelResetAll);
        restoreTimer = new CYFTimer(3, CancelRestore);

        Save.onClick.AddListener(() => {
            if (listening != null)
                StopListening();
            SaveKeybinds();
        });
        ResetAll.onClick.AddListener(() => {
            if (listening != null)
                StopListening();
            if (resetAllTimer.IsElapsing()) {
                resetAllTimer.Stop();
                ResetAll.GetComponentInChildren<Text>().text = "Reset All";
                Reload(true);
            } else {
                resetAllTimer.Start();
                ResetAll.GetComponentInChildren<Text>().text = "You sure?";
            }
        });
        Restore.onClick.AddListener(() => {
            if (listening != null)
                StopListening();
            if (restoreTimer.IsElapsing()) {
                restoreTimer.Stop();
                Restore.GetComponentInChildren<Text>().text = "Restore";
                FactoryResetKeybinds();
            } else {
                restoreTimer.Start();
                Restore.GetComponentInChildren<Text>().text = "You sure?";
            }
        });
        Back.onClick.AddListener(() => {
            if (listening != null)
                StopListening();
            SceneManager.LoadScene("Options");
        });

        Reload();
    }

    public void CancelResetAll() {
        ResetAll.GetComponentInChildren<Text>().text = "Reset All";
    }
    public void CancelRestore() {
        Restore.GetComponentInChildren<Text>().text = "Restore";
    }

    public void LoadKeybinds() {
        KeyboardInput.LoadPlayerKeys();
        tempKeybinds = new Dictionary<string, List<KeyCode>>(KeyboardInput.generalKeys);
        foreach (KeybindEntry keybind in new KeybindEntry[] { Confirm, Cancel, Menu, Up, Left, Down, Right })
            UpdateKeyList(keybind);
        UpdateColor();
    }

    public void SaveKeybinds() {
        string invalidReason = null;

        Dictionary<KeyCode, string[]> conflicts = KeyboardInput.GetConflicts(tempKeybinds);
        if (conflicts.Count > 0) {
            string[] conflict = conflicts[conflicts.Keys.First()];
            invalidReason = "Please get rid of key conflicts before saving this configuration.";
        }

        if (invalidReason == null)
            foreach (KeyValuePair<string, List<KeyCode>> p in tempKeybinds)
                if (p.Value.Count == 0) {
                    invalidReason = "The keybind \"" + p.Key + "\" is unbound! Please add at least one key to it.";
                    break;
                }

        if (invalidReason != null) {
            UnitaleUtil.PlaySound("Reset", "hurtsound");
            HijackListeningText(invalidReason, "ff0000");
            return;
        }

        KeyboardInput.SaveKeybinds(tempKeybinds);
        Reload();

        UnitaleUtil.PlaySound("Save", "saved");
        HijackListeningText("Keybinds saved!");
    }

    public void HijackListeningText(string text, string color = "ffff00") {
        Listening.text = "<color=#" + color + ">" + text + "</color>";
        if (textHijackTimer.IsElapsing())
            textHijackTimer.Stop();
        textHijackTimer.Start();
    }

    public void Reload(bool isReset = false) {
        LoadKeybinds();

        if (isReset) {
            UnitaleUtil.PlaySound("Reset", "hurtsound");
            HijackListeningText("Keybinds reset!");
        }
    }

    public void FactoryResetKeybinds() {
        tempKeybinds = new Dictionary<string, List<KeyCode>>(KeyboardInput.defaultKeys);
        foreach (KeybindEntry keybind in new KeybindEntry[] { Confirm, Cancel, Menu, Up, Left, Down, Right })
            UpdateKeyList(keybind);
        UpdateColor();

        UnitaleUtil.PlaySound("Reset", "hurtsound");
        HijackListeningText("Keybinds restored to their default state!");
    }

    public void UpdateColor() {
        List<string> conflictingKeybinds = new List<string>();
        Dictionary<KeyCode, string[]> conflicts = KeyboardInput.GetConflicts(tempKeybinds);
        foreach (string[] conflictArray in conflicts.Values)
            foreach (string conflict in conflictArray)
                if (!conflictingKeybinds.Contains(conflict))
                    conflictingKeybinds.Add(conflict);

        foreach (KeybindEntry keybind in new KeybindEntry[] { Confirm, Cancel, Menu, Up, Left, Down, Right }) {
            Color c;
            if (listening != null && listening.Name == keybind.Name)                                     c = new Color(1, 1, 0);
            else if (tempKeybinds[keybind.Name].Count == 0)                                              c = new Color(1, 0, 0);
            else if (conflictingKeybinds.Contains(keybind.Name))                                         c = new Color(1, 0, 0);
            else if (!tempKeybinds[keybind.Name].SequenceEqual(KeyboardInput.generalKeys[keybind.Name])) c = new Color(1, 1, 1);
            else                                                                                         c = new Color(0.7f, 0.7f, 0.7f);
            keybind.SetColor(c);
        }
    }

    public void UpdateKeyList(KeybindEntry keybind) {
        keybind.SetKeyList(string.Join(", ", tempKeybinds[keybind.Name].Select(k => k.ToString()).ToArray()));
    }

    public void AddKeyToKeybind(KeybindEntry keybind, KeyCode key) {
        List<KeyCode> keys;
        tempKeybinds.TryGetValue(keybind.Name, out keys);
        keys.Add(key);
        tempKeybinds[keybind.Name] = keys;

        UpdateKeyList(keybind);
        UpdateColor();
    }

    public void RemoveKeyFromKeybind(KeybindEntry keybind, KeyCode key) {
        List<KeyCode> keys;
        tempKeybinds.TryGetValue(keybind.Name, out keys);
        keys.Remove(key);
        tempKeybinds[keybind.Name] = keys;

        UpdateKeyList(keybind);
        UpdateColor();
    }

    public void ResetKeybind(KeybindEntry keybind) {
        if (listening != null)
            StopListening();
        KeyboardInput.generalKeys[keybind.Name] = new List<KeyCode>(tempKeybinds[keybind.Name]);

        UpdateKeyList(keybind);
        UpdateColor();
    }

    public void ClearKeybind(KeybindEntry keybind) {
        if (listening != null)
            StopListening();

        tempKeybinds[keybind.Name].Clear();

        UpdateKeyList(keybind);
        UpdateColor();
    }

    public void StartListening(KeybindEntry keybind) {
        if (listening != null)
            StopListening();
        listening = keybind;
        listening.SetEditText("Stop");

        UpdateListeningText();
        UpdateColor();
    }

    public void StopListening() {
        if (listening != null)
            listening.SetEditText("Edit");
        listening = null;

        UpdateListeningText();
        UpdateColor();
    }

    public void UpdateListeningText() {
        textHijackTimer.Stop();
        Dictionary<KeyCode, string[]> conflicts = KeyboardInput.GetConflicts(tempKeybinds);
        if (listening)
            Listening.text = "Listening for " + listening.Name + ". Press a key to add/remove it! ESC to stop.";
        else if (conflicts.Count == 0)
            Listening.text = "Not currently listening...";
        else {
            string[] conflict = conflicts[conflicts.Keys.First()];
            Listening.text = "Conflict detected: " + conflicts.Keys.First().ToString() + " used for both " + conflict[0] + " and " + conflict[1] + ".";
        }
    }

    void Update() {
        textHijackTimer.Update();
        resetAllTimer.Update();
        restoreTimer.Update();

        if (listening != null)
            foreach (KeyCode key in Enum.GetValues(typeof(KeyCode)))
                if (Input.GetKeyDown(key)) {
                    if (key == KeyCode.Escape)                           StopListening();
                    else if (tempKeybinds[listening.Name].Contains(key)) RemoveKeyFromKeybind(listening, key);
                    else if (key != KeyCode.Mouse0)                      AddKeyToKeybind(listening, key);
                }
    }
}
