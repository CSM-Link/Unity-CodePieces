using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// A replacement for Unity's PlayerPrefs that stores data in a JSON file.
/// </summary>
[Serializable]
public class JsonPlayerPrefs
{
	// EXAMPLE USAGE:
	// JsonPlayerPrefs prefs = new JsonPlayerPrefs(Application.persistentDataPath + "/Preferences.json");
	// prefs.SetInt("testKey", 18);
	// prefs.Save();
	// int i = prefs.GetInt("testKey");


	[Serializable]
	class PlayerPref
	{
		public string key;
		public string value;

		public PlayerPref(string key, string value)
		{
			this.key = key;
			this.value = value;
		}
	}


	[SerializeField] List<PlayerPref> playerPrefs = new List<PlayerPref>();
	string savePath;


	// Constructor
	public JsonPlayerPrefs(string savePath)
	{
		this.savePath = savePath;
		// try to load existing data
		if (File.Exists(savePath))
		{
			using (StreamReader reader = new StreamReader(savePath))
			{
				string json = reader.ReadToEnd();
				JsonPlayerPrefs data = JsonUtility.FromJson<JsonPlayerPrefs>(json);
				this.playerPrefs = data.playerPrefs;
			}
		}
	}


	/// <summary>
	/// Removes all keys and values from the preferences. Use with caution.
	/// </summary>
	public void DeleteAll()
	{
		playerPrefs.Clear();
	}


	/// <summary>
	/// Removes key and its corresponding value from the preferences.
	/// </summary>
	public void DeleteKey(string key)
	{
		for (int i = playerPrefs.Count - 1; i >= 0; i--) // in reverse since we're removing
		{
			if (playerPrefs[i].key == key)
			{
				playerPrefs.RemoveAt(i);
			}
		}
	}


	/// <summary>
	/// Returns the value corresponding to key in the preference file if it exists.
	/// </summary>
	public float GetFloat(string key, float defaultValue = 0f)
	{
		PlayerPref playerPref;
		if (TryGetPlayerPref(key, out playerPref))
		{
			float value;
			if (float.TryParse(playerPref.value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out value))
			{
				return value;
			}
		}
		return defaultValue;
	}


	/// <summary>
	/// Returns the value corresponding to key in the preference file if it exists.
	/// </summary>
	public int GetInt(string key, int defaultValue = 0)
	{
		PlayerPref playerPref;
		if (TryGetPlayerPref(key, out playerPref))
		{
			int value;
			if (int.TryParse(playerPref.value, out value))
			{
				return value;
			}
		}
		return defaultValue;
	}


	/// <summary>
	/// Returns the value corresponding to key in the preference file if it exists.
	/// </summary>
	public string GetString(string key, string defaultValue = "")
	{
		PlayerPref playerPref;
		if (TryGetPlayerPref(key, out playerPref))
		{
			return playerPref.value;
		}
		return defaultValue;
	}


	/// <summary>
	/// Returns true if key exists in the preferences.
	/// </summary>
	public bool HasKey(string key)
	{
		for (int i = 0; i < playerPrefs.Count; i++)
		{
			if (playerPrefs[i].key == key)
			{
				return true;
			}
		}
		return false;
	}


	/// <summary>
	/// Writes all modified preferences to disk.
	/// </summary>
	public void Save()
	{
		// create directory if it doesn't already exist
		string directory = Path.GetDirectoryName(@savePath);
		Directory.CreateDirectory(directory);
		// serialize and save file
		string json = JsonUtility.ToJson(this);
		using (StreamWriter writer = new StreamWriter(savePath))
		{
			writer.WriteLine(json);
		}
	}


	/// <summary>
	/// Sets the value of the preference identified by key.
	/// </summary>
	public void SetFloat(string key, float value)
	{
		SetString(key, value.ToString());
	}


	/// <summary>
	/// Sets the value of the preference identified by key.
	/// </summary>
	public void SetInt(string key, int value)
	{
		SetString(key, value.ToString());
	}


	/// <summary>
	/// Sets the value of the preference identified by key.
	/// </summary>
	public void SetString(string key, string value)
	{
		PlayerPref playerPref;
		if (TryGetPlayerPref(key, out playerPref))
		{
			playerPref.value = value;
		}
		else
		{
			playerPrefs.Add(new PlayerPref(key, value));
		}
	}


	bool TryGetPlayerPref(string key, out PlayerPref playerPref)
	{
		playerPref = null;
		for (int i = 0; i < playerPrefs.Count; i++)
		{
			if (playerPrefs[i].key == key)
			{
				playerPref = playerPrefs[i];
				return true;
			}
		}
		return false;
	}
}