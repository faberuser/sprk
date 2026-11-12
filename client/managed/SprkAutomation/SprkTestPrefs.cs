using System;
using System.Collections.Generic;
using UnityEngine;

public static class SprkTestPrefs
{
	private struct IntPreference
	{
		public int Default;

		public int Value;
	}

	private struct StringPreference
	{
		public string Default;

		public string Value;
	}

	private static readonly Dictionary<string, IntPreference> graphicsInts = new Dictionary<string, IntPreference>();

	private static readonly Dictionary<string, StringPreference> graphicsStrings = new Dictionary<string, StringPreference>();

	private static readonly Dictionary<string, bool> graphicsKeys = new Dictionary<string, bool>();

	public static int GraphicsStorageReads { get; private set; }

	private static bool CacheGraphics(string key)
	{
		if (!key.StartsWith("Sprk.Graphics.", StringComparison.Ordinal) && !key.StartsWith("KingsRaid_SystemOption_graphic", StringComparison.Ordinal) && !key.StartsWith("KingsRaid_GameOption_render_", StringComparison.Ordinal))
		{
			return key.StartsWith("KingsRaid_SystemOption_render", StringComparison.Ordinal);
		}
		return true;
	}

	private static void Invalidate(string key)
	{
		graphicsInts.Remove(key);
		graphicsStrings.Remove(key);
		graphicsKeys.Remove(key);
	}

	public static void ClearGraphicsCache()
	{
		graphicsInts.Clear();
		graphicsStrings.Clear();
		graphicsKeys.Clear();
	}

	public static string DeviceId()
	{
		if (!(Prefix() == ""))
		{
			return "sprk-test-" + Environment.GetEnvironmentVariable("SPRK_AUTOMATION_PROFILE");
		}
		return SystemInfo.deviceUniqueIdentifier;
	}

	private static string Prefix()
	{
		SprkStorageMigration.Ensure();
		if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SPRK_AUTOMATION_DIR")))
		{
			return "";
		}
		string environmentVariable = Environment.GetEnvironmentVariable("SPRK_AUTOMATION_PROFILE");
		if (!string.IsNullOrEmpty(environmentVariable))
		{
			return "SprkAutomation/" + environmentVariable + "/";
		}
		return "";
	}

	private static string Key(string key)
	{
		return Prefix() + key;
	}

	public static string GetString(string key)
	{
		return GetString(key, "");
	}

	public static string GetString(string key, string value)
	{
		string key2 = Key(key);
		if (!CacheGraphics(key))
		{
			return PlayerPrefs.GetString(key2, value);
		}
		if (graphicsStrings.TryGetValue(key2, out var value2) && value2.Default == value)
		{
			return value2.Value;
		}
		GraphicsStorageReads++;
		string text = PlayerPrefs.GetString(key2, value);
		graphicsStrings[key2] = new StringPreference
		{
			Default = value,
			Value = text
		};
		return text;
	}

	public static int GetInt(string key)
	{
		return GetInt(key, 0);
	}

	public static int GetInt(string key, int value)
	{
		string key2 = Key(key);
		if (!CacheGraphics(key))
		{
			return PlayerPrefs.GetInt(key2, value);
		}
		if (graphicsInts.TryGetValue(key2, out var value2) && value2.Default == value)
		{
			return value2.Value;
		}
		GraphicsStorageReads++;
		int num = PlayerPrefs.GetInt(key2, value);
		graphicsInts[key2] = new IntPreference
		{
			Default = value,
			Value = num
		};
		return num;
	}

	public static float GetFloat(string key)
	{
		return PlayerPrefs.GetFloat(Key(key));
	}

	public static float GetFloat(string key, float value)
	{
		return PlayerPrefs.GetFloat(Key(key), value);
	}

	public static void SetString(string key, string value)
	{
		string key2 = Key(key);
		PlayerPrefs.SetString(key2, value);
		Invalidate(key2);
	}

	public static void SetInt(string key, int value)
	{
		string key2 = Key(key);
		PlayerPrefs.SetInt(key2, value);
		Invalidate(key2);
	}

	public static void SetFloat(string key, float value)
	{
		string key2 = Key(key);
		PlayerPrefs.SetFloat(key2, value);
		Invalidate(key2);
	}

	public static bool HasKey(string key)
	{
		string key2 = Key(key);
		if (!CacheGraphics(key))
		{
			return PlayerPrefs.HasKey(key2);
		}
		if (graphicsKeys.TryGetValue(key2, out var value))
		{
			return value;
		}
		GraphicsStorageReads++;
		value = PlayerPrefs.HasKey(key2);
		graphicsKeys[key2] = value;
		return value;
	}

	public static void DeleteKey(string key)
	{
		string key2 = Key(key);
		PlayerPrefs.DeleteKey(key2);
		Invalidate(key2);
	}

	public static void Save()
	{
		PlayerPrefs.Save();
	}

	public static void DeleteAll()
	{
		if (Prefix() != "")
		{
			throw new InvalidOperationException("DeleteAll is disabled for test profiles; choose a new profile name");
		}
		PlayerPrefs.DeleteAll();
		ClearGraphicsCache();
	}
}
