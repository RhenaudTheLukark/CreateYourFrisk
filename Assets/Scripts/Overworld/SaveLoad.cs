using System;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

/// <summary>
/// A static class that is used to load and save a gamestate.
/// </summary>
public static class SaveLoad {
    public static GameState savedGame;                     // The save
    public static AlMightyGameState almightycurrentGame;   // The almighty save
    public static bool started;

    public static void Start() {
        started = true;
        try {
            if (File.Exists(Application.persistentDataPath + "/save.gd")) {
                Debug.Log("We found a save at this location : " + Application.persistentDataPath + "/save.gd");
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Open(Application.persistentDataPath + "/save.gd", FileMode.Open);
                savedGame = (GameState)bf.Deserialize(file);
                if (savedGame.CYFversion == null || string.Compare(savedGame.CYFversion, GlobalControls.OverworldVersion, StringComparison.OrdinalIgnoreCase) < 0)
                    throw new CYFException("Your save file is from <b>CYF v" + (savedGame.CYFversion ?? "0.6.3 or earlier") + "</b>, "
                  + "but you are currently running <b>CYF v" + GlobalControls.CYFversion + "</b>. Your save is incompatible with this version of CYF.\n\n"
                  + "To fix this, you must delete your save file. It can be found here: \n<b>"
                  + Application.persistentDataPath + "/save.gd</b>\n\n"
                  + "Or, you can <b>Press R now</b> to delete your save and close CYF.\n"
                  + "Tell me if you have any more problems, and thanks for following my fork! ^^");
                file.Close();
            } else
                Debug.Log("There's no save at all.");
        } catch (CYFException c) {
            GlobalControls.allowWipeSave = true;
            UnitaleUtil.DisplayLuaError(StaticInits.ENCOUNTER, c.Message, true);
        } catch (Exception e) {
            GlobalControls.allowWipeSave = true;
            UnitaleUtil.DisplayLuaError(StaticInits.ENCOUNTER, "Have you saved on a previous or newer version of CYF? Your save isn't compatible with this version.\n\n"
           + "To fix this, you must delete your save file. It can be found here: \n<b>"
           + Application.persistentDataPath + "/save.gd</b>\n\n"
           + "Or, you can <b>Press R now</b> to delete your save and close CYF.\n"
           + "Tell me if you have any more problems, and thanks for following my fork! ^^\n\nError encountered:\n"
           + e.Message + "\n" + e.StackTrace, true);
        }
    }

    public static void Save(bool saveMapState = false) {
        if (saveMapState)
            EventManager.instance.SetEventStates(true);
        GameState currentGame = new GameState();
        currentGame.SaveGameVariables();
        BinaryFormatter bf = new BinaryFormatter();
        //Application.persistentDataPath is a string, so if you wanted you can put that into unitaleutil.writeinlog if you want to know where save games are located
        FileStream file = File.Create(Application.persistentDataPath + "/save.gd");
        bf.Serialize(file, currentGame);
        savedGame = currentGame;
        Debug.Log("Save created at this location : " + Application.persistentDataPath + "/save.gd");
        file.Close();
    }

    public static bool Load(bool loadGlobals = true) {
        if (File.Exists(Application.persistentDataPath + "/save.gd")) {
            Debug.Log("We found a save at this location : " + Application.persistentDataPath + "/save.gd");
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/save.gd", FileMode.Open);
            GameState currentGame = (GameState)bf.Deserialize(file);
            currentGame.LoadGameVariables(loadGlobals);
            file.Close();
            return true;
        }
        Debug.Log("There's no save to load.");
        savedGame = null;
        return false;
    }

    public static void SaveAlMighty(string key = null) {
        almightycurrentGame = new AlMightyGameState();
        almightycurrentGame.UpdateVariables();
        File.Delete(Application.persistentDataPath + "/AlMightySave.gd");
        BinaryFormatter bf = new BinaryFormatter();
        //Application.persistentDataPath is a string, so if you wanted you can put that into unitaleutil.writeinlog if you want to know where save games are located
        FileStream file = File.Create(Application.persistentDataPath + "/AlMightySave.gd");
        bf.Serialize(file, almightycurrentGame);
        Debug.Log(key == null ? "AlMighties have been saved!" : "The AlMighty \"" + key + "\" has been saved!");
        file.Close();
    }

    public static bool LoadAlMighty() {
        if (File.Exists(Application.persistentDataPath + "/AlMightySave.gd")) {
            Debug.Log("We found an almighty save at this location : " + Application.persistentDataPath + "/AlMightySave.gd");
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/AlMightySave.gd", FileMode.Open);
            almightycurrentGame = (AlMightyGameState)bf.Deserialize(file);
            almightycurrentGame.LoadVariables();
            file.Close();
            return true;
        }
        Debug.Log("There's no almighty save to load.");
        return false;
    }
}
