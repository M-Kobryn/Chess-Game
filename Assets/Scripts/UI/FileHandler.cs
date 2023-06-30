using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class FileHandler
{
    public static Action OnGameLoad;
    private static string dataPathDir = Application.persistentDataPath;

     public static void Save(string fileName, object gameData) 
     {
        string path = Path.Combine(dataPathDir, fileName);
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        string data = JsonUtility.ToJson(gameData,true );

        using (FileStream stream = new FileStream(path, FileMode.Create)) 
        {
            using (StreamWriter writer = new StreamWriter(stream)) 
            {
                writer.Write(data);
            }
        }
     }
    public static void Load(string fileName, GameLogic toWhere) 
    {
        string path = Path.Combine(dataPathDir, fileName);
        if (File.Exists(path)) 
        {
            string data = "";
            using (FileStream stream = new FileStream(path, FileMode.Open))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    data = reader.ReadToEnd();
                }
            }
            JsonUtility.FromJsonOverwrite(data, toWhere);
            OnGameLoad?.Invoke();
        }
    }
}
