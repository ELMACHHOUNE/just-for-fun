using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace LWalida
{
    public class SaveManager : MonoBehaviour
    {
        public const int CurrentVersion = 1;
        private const string FileName = "lwalida_save.json";

        public string SavePath => Path.Combine(Application.persistentDataPath, FileName);

        public bool HasValidSave()
        {
            return TryLoad(out _, out _);
        }

        public bool TryLoad(out SaveData data, out string error)
        {
            data = null;
            error = null;
            try
            {
                if (!File.Exists(SavePath))
                {
                    error = "no save file";
                    return false;
                }

                string json = File.ReadAllText(SavePath);
                if (string.IsNullOrWhiteSpace(json))
                {
                    error = "empty save file";
                    return false;
                }

                data = Parse(json);
                if (data == null)
                {
                    error = "corrupt save";
                    return false;
                }

                return true;
            }
            catch (Exception exception)
            {
                error = exception.Message;
                data = null;
                return false;
            }
        }

        public static SaveData Parse(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return null;
            try
            {
                var data = JsonUtility.FromJson<SaveData>(json);
                if (data == null) return null;
                if (data.version != CurrentVersion) return null;
                if (string.IsNullOrEmpty(data.sceneId) || !SceneIds.IsKnown(data.sceneId)) return null;
                if (data.memoryIds == null) data.memoryIds = new List<string>();
                if (data.flags == null) data.flags = new List<string>();
                if (data.settings == null) data.settings = new SaveSettingsData();

                var unique = new HashSet<string>();
                var cleanedMemories = new List<string>();
                for (int i = 0; i < data.memoryIds.Count; i++)
                {
                    string id = data.memoryIds[i];
                    if (string.IsNullOrEmpty(id)) continue;
                    bool known = false;
                    for (int m = 0; m < MemoryIds.All.Length; m++)
                    {
                        if (MemoryIds.All[m] == id) { known = true; break; }
                    }
                    if (known && unique.Add(id)) cleanedMemories.Add(id);
                }
                data.memoryIds = cleanedMemories;
                return data;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public void Save(string sceneId, IEnumerable<string> memories, IEnumerable<string> flags)
        {
            if (string.IsNullOrEmpty(sceneId)) return;
            try
            {
                var data = new SaveData
                {
                    version = CurrentVersion,
                    sceneId = sceneId,
                    savedAtUtc = DateTime.UtcNow.ToString("o"),
                    settings = SettingsService.Snapshot(),
                    memoryIds = new List<string>(),
                    flags = new List<string>()
                };

                if (memories != null)
                {
                    foreach (string id in memories)
                    {
                        if (!string.IsNullOrEmpty(id)) data.memoryIds.Add(id);
                    }
                }

                if (flags != null)
                {
                    foreach (string flag in flags)
                    {
                        if (!string.IsNullOrEmpty(flag)) data.flags.Add(flag);
                    }
                }

                string json = JsonUtility.ToJson(data, true);
                string directory = Path.GetDirectoryName(SavePath);
                if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);

                string tempPath = SavePath + ".tmp";
                File.WriteAllText(tempPath, json);
                if (File.Exists(SavePath))
                {
                    try
                    {
                        File.Replace(tempPath, SavePath, null);
                    }
                    catch (Exception)
                    {
                        File.Copy(tempPath, SavePath, true);
                        File.Delete(tempPath);
                    }
                }
                else
                {
                    File.Move(tempPath, SavePath);
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[LWalida] Could not save: " + exception.Message);
            }
        }

        public void Delete()
        {
            try
            {
                if (File.Exists(SavePath)) File.Delete(SavePath);
                string tempPath = SavePath + ".tmp";
                if (File.Exists(tempPath)) File.Delete(tempPath);
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[LWalida] Could not delete save: " + exception.Message);
            }
        }
    }
}
