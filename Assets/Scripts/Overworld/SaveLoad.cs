﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

/// <summary>
/// A static class that is used to load and save a gamestate.
/// </summary>
public static class SaveLoad {
    public static GameState savedGame = null;                     //The save
    public static AlMightyGameState almightycurrentGame = null;   //The almighty save
    public static bool started = false;
    
    public static void Start() {
        started = true;
        try {
            if (File.Exists(Application.persistentDataPath + "/save.gd")) {
                Debug.Log("We found a save at this location : " + Application.persistentDataPath + "/save.gd");
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Open(Application.persistentDataPath + "/save.gd", FileMode.Open);
                savedGame = (GameState)bf.Deserialize(file);
                file.Close();
            } else {
                Debug.Log("There's no save at all.");
            }
        } catch {
            UnitaleUtil.DisplayLuaError(StaticInits.ENCOUNTER, "Have you saved on a previous version of CYF? Your save isn't compatible with this version.\n\n"
           + "To fix this, you must delete your save file. It can be found here: \n<b>"
           + Application.persistentDataPath + "/save.gd</b>\n"
           + "Tell me if you have any more problems, and thanks for following my fork! ^^\n\n"
           + "PS: Don't try to press ESCAPE, or bad things can happen ;)");
        }
    }

    public static void Save() {
        GameState currentGame = new GameState();
        currentGame.SaveGameVariables();
        BinaryFormatter bf = new BinaryFormatter();
        //Application.persistentDataPath is a string, so if you wanted you can put that into unitaleutil.writeinlog if you want to know where save games are located
        FileStream file;
        file = File.Create(Application.persistentDataPath + "/save.gd");
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
        } else {
            Debug.Log("There's no save to load.");
            savedGame = null;
            return false;
        }
    }

    public static void SaveAlMighty() {
        almightycurrentGame = new AlMightyGameState();
        almightycurrentGame.UpdateVariables();
        File.Delete(Application.persistentDataPath + "/AlMightySave.gd");
        BinaryFormatter bf = new BinaryFormatter();
        //Application.persistentDataPath is a string, so if you wanted you can put that into unitaleutil.writeinlog if you want to know where save games are located
        FileStream file;
        file = File.Create(Application.persistentDataPath + "/AlMightySave.gd");
        bf.Serialize(file, almightycurrentGame);
        Debug.Log("AlMighty Save created at this location : " + Application.persistentDataPath + "/AlMightySave.gd");
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
        } else {
            Debug.Log("There's no almighty save to load.");
            return false;
        }
    }
}
