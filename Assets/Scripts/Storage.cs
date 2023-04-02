using UnityEngine;
namespace xUnityTools.WinnerPick
{
    /// <summary>
    /// Save/Load data from player prefs
    /// </summary>
    public class Storage
    {
        const string storageName = "SavedValues";

        public SavedValues Load()
        {
            string savedData = PlayerPrefs.GetString(storageName);

            if (string.IsNullOrEmpty(savedData))
            {
                // Saved data doesn't exist, so load default values
                SavedValues defaultValues = new SavedValues();

                // Serialize default values to JSON
                string jsonData = JsonUtility.ToJson(defaultValues);

                // Save serialized JSON data to PlayerPrefs
                PlayerPrefs.SetString("savedValues", jsonData);

                // Use default values
                return defaultValues;
            }
            else
            {
                // Saved data exists, so deserialize JSON data to object
                SavedValues loadedValues = JsonUtility.FromJson<SavedValues>(savedData);

                // Use loaded values
                return loadedValues;
            }
        }

        public void Save(SavedValues savedValues)
        {
            if (savedValues != null)
            {
                string json = JsonUtility.ToJson(savedValues);
                PlayerPrefs.SetString(storageName, json);
            }
        }
    }
}