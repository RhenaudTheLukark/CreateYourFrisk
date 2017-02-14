using System.IO;
using UnityEngine;

public class UndertaleSaveReader : MonoBehaviour
{
    // Use this for initialization
    private void Start()
    {
        FileStream f = System.IO.File.OpenRead(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "UNDERTALE/undertale.ini"));
        StreamReader r = new StreamReader(f);
        UnitaleUtil.writeInLog(r.ReadToEnd());
    }
}