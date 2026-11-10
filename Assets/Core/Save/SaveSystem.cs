using System.IO;
using UnityEngine;

namespace MobileParadise.Core.Save
{
    /// <summary>
    /// Reads and writes JSON files in persistentDataPath.
    /// One file per key (e.g. "settings", "memory_progress").
    /// </summary>
    public static class SaveSystem
    {
        private static string GetPath(string key)
        {
            return Path.Combine(Application.persistentDataPath, key + ".json");
        }

        public static bool Exists(string key)
        {
            return File.Exists(GetPath(key));
        }

        public static T Load<T>(string key) where T : new()
        {
            string path = GetPath(key);
            if (!File.Exists(path))
            {
                return new T();
            }

            try
            {
                return JsonUtility.FromJson<T>(File.ReadAllText(path));
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("SaveSystem: failed to load '" + key + "': " + e.Message);
                return new T();
            }
        }

        public static void Save<T>(string key, T data)
        {
            string path = GetPath(key);
            string tempPath = path + ".tmp";

            // Write to a temp file first so a crash never corrupts the real save.
            File.WriteAllText(tempPath, JsonUtility.ToJson(data, true));
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            File.Move(tempPath, path);
        }
    }
}
