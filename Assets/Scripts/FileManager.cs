using System.Collections.Generic;
using UnityEngine;
using System.IO;

public static class FileManager 
{
    public static List<string> GetFileNames()
    {
        List<string> jsonFiles = new List<string>(Directory.GetFiles(Application.persistentDataPath, "*.json"));
        for (int i = 0; i < jsonFiles.Count; i++)
        {
            jsonFiles[i] = Path.GetFileNameWithoutExtension(jsonFiles[i]);
        }
        return jsonFiles;
    }
    public static List<ReplayData> GetReplayDatas()
    {
        List<ReplayData> replayDatas = new List<ReplayData>();
        List<string> jsonFiles = new List<string>(Directory.GetFiles(Application.persistentDataPath, "*.json"));
        foreach (string jsonFile in jsonFiles)
        {
            string json = File.ReadAllText(jsonFile);
            ReplayData replayData = JsonUtility.FromJson<ReplayData>(json);
            replayDatas.Add(replayData);
        }
        return replayDatas;
    }
    public static void SaveReplayData(ReplayData replayData)
    {
        string json = JsonUtility.ToJson(replayData, true);
        File.WriteAllText(Application.persistentDataPath + "/replay_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".json", json);
    }
}
