using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

/// <summary>
/// Class handling IO system operations when saving and loading jsons.
/// </summary>
public static class SaveSystem
{
    private static readonly string SAVE_FOLDER = Application.dataPath + "/Saves/";
    public static void Init(){
        if (!Directory.Exists(SAVE_FOLDER))
        {
            Debug.Log("Creating save directory.");
            Directory.CreateDirectory(SAVE_FOLDER);
        }
    }
    /// <summary>
    /// Save string into specified file.
    /// </summary>
    /// <param name="saveString">String about to be saved. Json serialized.</param>
    /// <param name="fileName">Save filename.</param>
    public static void Save(string saveString, string fileName){
        File.WriteAllText(SAVE_FOLDER + "/"+fileName,saveString);
    }
    /// <summary>
    /// Load string(json serialized) from specified file.
    /// </summary>
    /// <param name="fileName">Save filename</param>
    /// <returns>Retriegved string when save is found, or null when it isn't.</returns>
    public static string Load(string fileName){
        if (File.Exists(SAVE_FOLDER + "/"+fileName))
        {
            string saveString = File.ReadAllText(SAVE_FOLDER + "/"+fileName);
            return saveString;
        }else{
            return null;
        }
    }
}
