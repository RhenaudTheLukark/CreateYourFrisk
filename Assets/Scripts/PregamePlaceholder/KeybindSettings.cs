using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class KeybindSettings : MonoBehaviour {
    public KeybindEntry Confirm, Cancel, Menu, Up, Left, Down, Right;
    public Text Listening;
    public Button Save, ResetAll, Restore, Back;

    private Dictionary<string, KeyCode[]> tempKeybinds = new Dictionary<string, KeyCode[]>(KeyboardInput.generalKeys);

    private Timer saveTimer;
    private Timer resetAllTimer;
    private Timer restoreTimer;

    [HideInInspector] public KeybindEntry listening = null;

    void Start() {
        Save.onClick.AddListener(() => {
            if (listening != null)
                StopListening();
            SaveKeybinds();
        });
        ResetAll.onClick.AddListener(() => {
            if (listening != null)
                StopListening();
            if (resetAllTimer != null) {
                ResetAll.GetComponentInChildren<Text>().text = "Reset All";
                Reload(true);
                resetAllTimer.Elapsed -= CancelResetAll;
                resetAllTimer = null;
            } else {
                ResetAll.GetComponentInChildren<Text>().text = "You sure?";
                resetAllTimer = new Timer(3000);
                resetAllTimer.Elapsed += CancelResetAll;
                resetAllTimer.Start();
            }
        });
        Restore.onClick.AddListener(() => {
            if (listening != null)
                StopListening();
            if (restoreTimer != null) {
                Restore.GetComponentInChildren<Text>().text = "Restore";
                FactoryResetKeybinds();
                restoreTimer.Elapsed -= CancelRestore;
                restoreTimer = null;
            } else {
                Restore.GetComponentInChildren<Text>().text = "You sure?";
                restoreTimer = new Timer(3000);
                restoreTimer.Elapsed += CancelRestore;
                restoreTimer.Start();
            }
        });
        Back.onClick.AddListener(() => {
            if (listening != null)
                StopListening();
            SceneManager.LoadScene("Options");
        });

        Reload();
    }

    public void CancelResetAll(object source, ElapsedEventArgs e) {
        ResetAll.GetComponentInChildren<Text>().text = "Reset All";
        resetAllTimer.Elapsed -= CancelResetAll;
        resetAllTimer = null;
    }
    public void CancelRestore(object source, ElapsedEventArgs e) {
        Restore.GetComponentInChildren<Text>().text = "Restore";
        restoreTimer.Elapsed -= CancelRestore;
        restoreTimer = null;
    }

    public void LoadKeybinds() {
        KeyboardInput.LoadPlayerKeys();
        tempKeybinds = new Dictionary<string, KeyCode[]>(KeyboardInput.generalKeys);
        foreach (KeybindEntry keybind in new KeybindEntry[] { Confirm, Cancel, Menu, Up, Left, Down, Right })
            UpdateKeyList(keybind);
        UpdateColor();
    }

    public void SaveKeybinds() {
        KeyboardInput.SaveKeybinds(tempKeybinds);
        Reload();

        UnitaleUtil.PlaySound("Save", "saved");
        HijackListeningText("Keybinds saved!");
    }

    public void HijackListeningText(string text) {
        Listening.text = "<color=#ffff00>" + text + "</color>";
        if (saveTimer != null)
            saveTimer.Elapsed -= UpdateListeningText;
        saveTimer = new Timer(5000);
        saveTimer.Elapsed += UpdateListeningText;
        saveTimer.Start();
    }

    public void Reload(bool isReset = false) {
        LoadKeybinds();

        if (isReset) {
            UnitaleUtil.PlaySound("Reset", "hurtsound");
            HijackListeningText("Keybinds reset!");
        }
    }

    public void FactoryResetKeybinds() {
        tempKeybinds = new Dictionary<string, KeyCode[]>(KeyboardInput.defaultKeys);
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
        KeyCode[] keys;
        tempKeybinds.TryGetValue(keybind.Name, out keys);
        List<KeyCode> keysList = keys.ToList();
        keysList.Add(key);
        tempKeybinds[keybind.Name] = keysList.ToArray();

        UpdateKeyList(keybind);
        UpdateColor();
    }

    public void RemoveKeyFromKeybind(KeybindEntry keybind, KeyCode key) {
        KeyCode[] keys;
        tempKeybinds.TryGetValue(keybind.Name, out keys);
        List<KeyCode> keysList = keys.ToList();
        keysList.Remove(key);
        tempKeybinds[keybind.Name] = keysList.ToArray();

        UpdateKeyList(keybind);
        UpdateColor();
    }

    public void ResetKeybind(KeybindEntry keybind) {
        if (listening != null)
            StopListening();
        KeyboardInput.generalKeys[keybind.Name].CopyTo(tempKeybinds[keybind.Name], 0);

        UpdateKeyList(keybind);
        UpdateColor();
    }

    public void ClearKeybind(KeybindEntry keybind) {
        if (listening != null)
            StopListening();

        tempKeybinds[keybind.Name] = new KeyCode[0];

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

    public void UpdateListeningText(object source, ElapsedEventArgs e) {
        saveTimer.Elapsed -= UpdateListeningText;
        saveTimer = null;
        UpdateListeningText();
    }
    public void UpdateListeningText() {
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
        if (listening != null)
            foreach (KeyCode key in Enum.GetValues(typeof(KeyCode)))
                if (Input.GetKeyDown(key)) {
                    if (key == KeyCode.Escape)                           StopListening();
                    else if (tempKeybinds[listening.Name].Contains(key)) RemoveKeyFromKeybind(listening, key);
                    else if (key != KeyCode.Mouse0)                      AddKeyToKeybind(listening, key);
                }
    }
}
