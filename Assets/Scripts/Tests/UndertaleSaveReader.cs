using System;
using System.IO;
using UnityEngine;

public class UndertaleSaveReader : MonoBehaviour {
    private void Start() {
        FileStream f = File.OpenRead(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "UNDERTALE/undertale.ini"));
        StreamReader r = new StreamReader(f);
        Debug.Log(r.ReadToEnd());
    }
}