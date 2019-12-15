using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class VideoRegistry : MonoBehaviour {

    private static Dictionary<string, FileInfo> dictDefault = new Dictionary<string, FileInfo>();
    private static Dictionary<string, FileInfo> dictMod = new Dictionary<string, FileInfo>();
    //public static Video GENERIC_VIDEO_PREFAB;

    // Use this for initialization
    void Start () {
        //GENERIC_VIDEO_PREFAB = Resources.Load<Image>("Prefabs/VideoContainer");
        loadAllFrom("Videos", true);
    }

    private static void loadAllFrom(string directoryPath, bool mod = false)
    {
        DirectoryInfo dInfo = new DirectoryInfo(directoryPath);
        FileInfo[] fInfoTest;

        if (!dInfo.Exists)
        {
            UnitaleUtil.DisplayLuaError("mod loading", "You tried to load the mod \"" + StaticInits.MODFOLDER + "\" but it can't be found, or at least its \"Videos\" folder can't be found.\nAre you sure it exists?");
        }

        fInfoTest = dInfo.GetFiles("*.mp4", SearchOption.AllDirectories);

        if (mod)
        {
            dictMod.Clear();
            foreach (FileInfo file in fInfoTest)
                dictMod[FileLoader.getRelativePathWithoutExtension(directoryPath, file.FullName).ToLower()] = file;
        }
        else
        {
            dictDefault.Clear();
            foreach (FileInfo file in fInfoTest)
                dictDefault[FileLoader.getRelativePathWithoutExtension(directoryPath, file.FullName).ToLower()] = file;
        }
        /*foreach (FileInfo file in fInfoTest) {
            string imageName = FileLoader.getRelativePathWithoutExtension(directoryPath, file.FullName).ToLower();
            Sprite temp;
            dict.TryGetValue(imageName, out temp);

            if (dict.ContainsKey(imageName) && temp == SpriteUtil.fromFile(file.FullName) &&!mod)
                continue;
            else if (dict.ContainsKey(imageName))
                dict.Remove(imageName);

            //Set(script_prefix + scriptName, FileLoader.getTextFrom(file.FullName));
            //string imageName = FileLoader.getRelativePathWithoutExtension(directoryPath, file.FullName).ToLower();
            //if (dict.ContainsKey(imageName))
            //    continue;

            dict.Add(imageName, SpriteUtil.fromFile(file.FullName));
        }*/
    }

    // Update is called once per frame
    void Update () {
		
	}
}
