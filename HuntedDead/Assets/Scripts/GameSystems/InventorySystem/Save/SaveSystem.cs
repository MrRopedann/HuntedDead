using System.IO;
using UnityEngine;

public static class SaveSystem
{
    static string PathFile => System.IO.Path.Combine(Application.persistentDataPath, "save.json");
    public static void Save(SaveDto dto) { 
        File.WriteAllText(PathFile, JsonUtility.ToJson(dto)); 
    }
    public static SaveDto Load() { 
        return File.Exists(PathFile) ? JsonUtility.FromJson<SaveDto>(File.ReadAllText(PathFile)) : null; 
    }
}
