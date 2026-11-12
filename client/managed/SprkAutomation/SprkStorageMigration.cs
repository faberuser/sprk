using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using UnityEngine;

public static class SprkStorageMigration
{
	public interface Preferences
	{
		string ReadReceipt();

		IEnumerable<string> LegacyNames();

		bool Contains(string name);

		void Copy(string name);

		void SaveReceipt(string value);
	}

	private sealed class RegistryPreferences : Preferences, IDisposable
	{
		private RegistryKey target;

		private readonly string oldKey;

		private readonly string receiptKey;

		public RegistryPreferences(string source = "Software\\masangsoft\\King's Raid", string receipt = "Sprk.Storage.MigrationV1Receipt")
		{
			oldKey = source;
			receiptKey = receipt;
			target = Registry.CurrentUser.CreateSubKey("Software\\sprk\\King's Raid");
		}

		public string ReadReceipt()
		{
			return target.GetValue(receiptKey) as string;
		}

		public IEnumerable<string> LegacyNames()
		{
			using RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(oldKey, writable: false);
			return (registryKey == null) ? new string[0] : registryKey.GetValueNames();
		}

		public bool Contains(string name)
		{
			return target.GetValue(name, null, RegistryValueOptions.DoNotExpandEnvironmentNames) != null;
		}

		public void Copy(string name)
		{
			using RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(oldKey, writable: false);
			if (registryKey == null)
			{
				return;
			}
			object value = registryKey.GetValue(name, null, RegistryValueOptions.DoNotExpandEnvironmentNames);
			if (value == null)
			{
				return;
			}
			RegistryValueKind valueKind = registryKey.GetValueKind(name);
			string value2 = null;
			switch (valueKind)
			{
			case RegistryValueKind.Binary:
			{
				byte[] array = (byte[])value;
				if (array.Length == 0 || array[^1] != 0)
				{
					throw new InvalidDataException("Unsupported native preference type.");
				}
				value2 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true).GetString(array, 0, array.Length - 1);
				break;
			}
			default:
				throw new InvalidDataException("Unsupported native preference type.");
			case RegistryValueKind.String:
			case RegistryValueKind.DWord:
				break;
			}
			target.SetValue(name, value, valueKind);
			string text = Regex.Replace(name, "_h[0-9]+$", "");
			switch (valueKind)
			{
			case RegistryValueKind.DWord:
			{
				int value3 = (int)value;
				switch (text)
				{
				case "KingsRaid_SystemOption_sound_BgmVolume":
				case "KingsRaid_SystemOption_sound_GameSoundVolume":
				case "KingsRaid_SystemOption_sound_CharacterVoiceVolume":
					PlayerPrefs.SetFloat(text, BitConverter.ToSingle(BitConverter.GetBytes(value3), 0));
					break;
				default:
					PlayerPrefs.SetInt(text, value3);
					break;
				}
				break;
			}
			case RegistryValueKind.Binary:
				PlayerPrefs.SetString(text, value2);
				break;
			case RegistryValueKind.String:
				PlayerPrefs.SetString(text, (string)value);
				break;
			default:
				throw new InvalidDataException("Unsupported native preference type.");
			}
		}

		public void SaveReceipt(string value)
		{
			PlayerPrefs.Save();
			target.SetValue(receiptKey, value, RegistryValueKind.String);
			target.Flush();
		}

		public void Dispose()
		{
			target.Dispose();
		}
	}

	[Serializable]
	public class Probe
	{
		public string company;

		public string path;

		public int value;
	}

	// Retired alongside the development branch's PostgreSQL migration.
	public const bool LegacyMigrationEnabled = false;

	public const string MarkerName = "sprk-storage-migration-v1.complete";

	private const string ReceiptValue = "Sprk.Storage.MigrationV1Receipt";

	private static bool ready;

	private static float retryAt;

	public static string Receipt;

	private static bool probed;

	private static SprkStorageReporter reporter;

	public static bool AllowedPreference(string name)
	{
		if (!name.StartsWith("Sprk.Graphics.", StringComparison.Ordinal) && !name.StartsWith("Sprk.Window.", StringComparison.Ordinal) && !name.StartsWith("KingsRaid_SystemOption_", StringComparison.Ordinal))
		{
			return name.StartsWith("KingsRaid_GameOption_", StringComparison.Ordinal);
		}
		return true;
	}

	private static void RejectLink(string path)
	{
		if ((File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0)
		{
			throw new IOException("Storage migration does not follow links.");
		}
	}

	private static void RejectAncestors(string path)
	{
		for (string text = Path.GetFullPath(path); text != null; text = Path.GetDirectoryName(text))
		{
			if (Directory.Exists(text) || File.Exists(text))
			{
				RejectLink(text);
			}
		}
	}

	private static void AtomicWrite(string destination, byte[] bytes)
	{
		string text = destination + ".migration-" + Guid.NewGuid().ToString("N") + ".tmp";
		try
		{
			using (FileStream fileStream = new FileStream(text, FileMode.CreateNew, FileAccess.Write, FileShare.None))
			{
				fileStream.Write(bytes, 0, bytes.Length);
				fileStream.Flush(flushToDisk: true);
			}
			if (!File.Exists(destination))
			{
				File.Move(text, destination);
			}
		}
		finally
		{
			if (File.Exists(text))
			{
				File.Delete(text);
			}
		}
	}

	private static int CopyAccounts(string oldRoot, string newRoot)
	{
		if (!Directory.Exists(oldRoot))
		{
			return 0;
		}
		RejectLink(oldRoot);
		int num = 0;
		List<string> list = new List<string>();
		string text = Path.Combine(oldRoot, "sprk-accounts.dat");
		if (File.Exists(text))
		{
			list.Add(text);
		}
		string path = Path.Combine(oldRoot, "SprkServers");
		if (Directory.Exists(path))
		{
			RejectLink(path);
			string[] directories = Directory.GetDirectories(path);
			foreach (string text2 in directories)
			{
				RejectLink(text2);
				if (Regex.IsMatch(Path.GetFileName(text2), "\\A[0-9A-Fa-f]{64}\\z"))
				{
					string text3 = Path.Combine(text2, "sprk-accounts.dat");
					if (File.Exists(text3))
					{
						list.Add(text3);
					}
				}
			}
		}
		foreach (string item in list)
		{
			RejectLink(item);
			string path2 = item.Substring(oldRoot.TrimEnd(Path.DirectorySeparatorChar).Length + 1);
			string text4 = Path.Combine(newRoot, path2);
			RejectAncestors(Path.GetDirectoryName(text4));
			Directory.CreateDirectory(Path.GetDirectoryName(text4));
			RejectLink(Path.GetDirectoryName(text4));
			if (File.Exists(text4))
			{
				RejectLink(text4);
				continue;
			}
			AtomicWrite(text4, File.ReadAllBytes(item));
			num++;
		}
		return num;
	}

	public static string ValidateReceipt(string receipt)
	{
		if (string.IsNullOrEmpty(receipt))
		{
			throw new InvalidDataException("Missing migration receipt.");
		}
		string[] array = receipt.Split('\n');
		if (array.Length != 6 || array[0] != "schema=1" || !array[1].StartsWith("installation=", StringComparison.Ordinal) || !Guid.TryParseExact(array[1].Substring(13), "N", out var _))
		{
			throw new InvalidDataException("Invalid migration receipt; old storage will not be imported again.");
		}
		for (int i = 3; i < 6; i++)
		{
			string[] array2 = array[i].Split('=');
			if (array2.Length != 2 || !int.TryParse(array2[1], out var result2) || result2 < 0)
			{
				throw new InvalidDataException("Invalid migration counters.");
			}
		}
		return receipt;
	}

	public static string Migrate(string oldRoot, string newRoot, Preferences preferences)
	{
		return Migrate(oldRoot, newRoot, preferences, LegacyMigrationEnabled);
	}

	public static string Migrate(string oldRoot, string newRoot, Preferences preferences, bool importLegacy)
	{
		RejectAncestors(newRoot);
		Directory.CreateDirectory(newRoot);
		RejectLink(newRoot);
		string text = Path.Combine(newRoot, "sprk-storage-migration-v1.complete");
		if (File.Exists(text))
		{
			RejectLink(text);
			string text2 = ValidateReceipt(File.ReadAllText(text));
			preferences.SaveReceipt(text2);
			return text2;
		}
		string text3 = preferences.ReadReceipt();
		if (!string.IsNullOrEmpty(text3))
		{
			text3 = ValidateReceipt(text3);
			AtomicWrite(text, Encoding.UTF8.GetBytes(text3));
			return text3;
		}
		using (new FileStream(Path.Combine(newRoot, "sprk-storage-migration-v1.lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
		{
			if (File.Exists(text))
			{
				return ValidateReceipt(File.ReadAllText(text));
			}
			text3 = preferences.ReadReceipt();
			if (!string.IsNullOrEmpty(text3))
			{
				text3 = ValidateReceipt(text3);
				AtomicWrite(text, Encoding.UTF8.GetBytes(text3));
				return text3;
			}
			int num = 0;
			int num2 = 0;
			bool flag = false;
			if (importLegacy)
			{
				flag = Directory.Exists(oldRoot);
				num = CopyAccounts(oldRoot, newRoot);
				foreach (string item in preferences.LegacyNames())
				{
					if (AllowedPreference(item))
					{
						flag = true;
						if (!preferences.Contains(item))
						{
							preferences.Copy(item);
							num2++;
						}
					}
				}
			}
			text3 = "schema=1\ninstallation=" + Guid.NewGuid().ToString("N") + "\ncompleted_utc=" + DateTime.UtcNow.ToString("o") + "\nfiles=" + num + "\npreferences=" + num2 + "\nsource=" + (flag ? 1 : 0);
			preferences.SaveReceipt(text3);
			AtomicWrite(text, Encoding.UTF8.GetBytes(text3));
			return text3;
		}
	}

	private static void NativeProbe()
	{
		string environmentVariable = Environment.GetEnvironmentVariable("SPRK_STORAGE_PROBE_DIR");
		string environmentVariable2 = Environment.GetEnvironmentVariable("SPRK_STORAGE_PROBE_ID");
		if (!probed && !string.IsNullOrEmpty(environmentVariable) && !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SPRK_AUTOMATION_DIR")) && Guid.TryParseExact(environmentVariable2, "N", out var _))
		{
			probed = true;
			using (RegistryPreferences preferences = new RegistryPreferences("Software\\SPRK\\StorageProbe\\" + environmentVariable2, "Sprk.Storage.ProbeReceipt." + environmentVariable2))
			{
				Migrate(Path.Combine(environmentVariable, "old"), Path.Combine(environmentVariable, "new"), preferences);
			}
			Probe obj = new Probe
			{
				company = Application.companyName,
				path = Application.persistentDataPath,
				value = PlayerPrefs.GetInt("Sprk.Graphics.StorageProbe." + environmentVariable2, -1)
			};
			File.WriteAllText(Path.Combine(environmentVariable, "native-result.json"), JsonUtility.ToJson(obj));
			Application.Quit();
		}
	}

	public static void Ensure()
	{
		NativeProbe();
		if (ready || Time.realtimeSinceStartup < retryAt || Application.companyName != "sprk" || !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SPRK_AUTOMATION_DIR")) || !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SPRK_CONQUEST_WORKER_KEY")))
		{
			return;
		}
		retryAt = Time.realtimeSinceStartup + 10f;
		try
		{
			string persistentDataPath = Application.persistentDataPath;
			string oldRoot = Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(persistentDataPath)), "masangsoft", "King's Raid");
			using (RegistryPreferences preferences = new RegistryPreferences())
			{
				Receipt = Migrate(oldRoot, persistentDataPath, preferences);
			}
			ready = true;
			Debug.Log("SPRK storage schema 1 ready; legacy import disabled for this profile.");
		}
		catch (Exception ex)
		{
			Debug.LogWarning("SPRK storage migration pending: " + ex.GetType().Name);
		}
	}

	public static void StartReporter()
	{
		Ensure();
		if (ready && !(reporter != null))
		{
			GameObject gameObject = new GameObject("SprkStorageReporter");
			UnityEngine.Object.DontDestroyOnLoad(gameObject);
			reporter = gameObject.AddComponent<SprkStorageReporter>();
		}
	}
}
