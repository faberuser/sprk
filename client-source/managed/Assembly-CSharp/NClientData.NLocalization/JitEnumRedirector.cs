using System.Collections.Generic;
using System.IO;
using NShared.NJit;
using NVespa;

namespace NClientData.NLocalization;

public class JitEnumRedirector
{
	public class MapFromCsv : Dictionary<string, string>
	{
	}

	private LocalizeLoadDefine _loadDefine;

	private ILocalizePacker _localizePacker;

	public bool IsLoaded { get; private set; }

	public JitEnumRedirector(LocalizeLoadDefine loadDefine)
	{
		_loadDefine = loadDefine;
	}

	public bool Load()
	{
		if (IsLoaded)
		{
			return true;
		}
		lock (this)
		{
			if (LoadInternal())
			{
				IsLoaded = true;
			}
			return IsLoaded;
		}
	}

	public bool Contains(string key)
	{
		if (_localizePacker == null)
		{
			return false;
		}
		string value;
		return _localizePacker.TryGet(key, out value);
	}

	public bool TryGet(string key, out string value)
	{
		if (_localizePacker == null)
		{
			value = string.Empty;
			return false;
		}
		return _localizePacker.TryGet(key, out value);
	}

	public void Reset()
	{
		_localizePacker = null;
	}

	public bool ForceRebuild()
	{
		MapFromCsv mapFromCsv = LoadFromCsv();
		if (mapFromCsv == null)
		{
			return false;
		}
		SerializeJit(mapFromCsv);
		return true;
	}

	public bool Test()
	{
		if (!LoadFromJit())
		{
			return false;
		}
		MapFromCsv mapFromCsv = LoadFromCsv();
		if (mapFromCsv == null)
		{
			return false;
		}
		foreach (KeyValuePair<string, string> item in mapFromCsv)
		{
			if (!_localizePacker.TryGet(item.Key, out var value))
			{
				return false;
			}
			if (item.Value != value)
			{
				return false;
			}
		}
		return true;
	}

	private bool LoadInternal()
	{
		if (LoadFromJit())
		{
			return true;
		}
		if (JitOption.LoadJitOnly)
		{
			VespaDebug.LogError($"JitEnumRedirector.LoadFromJit is Failed: {_loadDefine.GetFullPath_for_Jit()}");
			return false;
		}
		MapFromCsv mapFromCsv = LoadFromCsv();
		if (mapFromCsv == null)
		{
			VespaDebug.LogError($"JitEnumRedirector.LoadFromCsv is Failed: {_loadDefine.GetFullPath_for_Jit()}");
			return false;
		}
		SerializeJit(mapFromCsv);
		return true;
	}

	private bool LoadFromJit()
	{
		if (ForceRebuildJit.Rebuild)
		{
			return false;
		}
		if (!File.Exists(_loadDefine.GetFullPath_for_Jit()))
		{
			return false;
		}
		byte[] array = File.ReadAllBytes(_loadDefine.GetFullPath_for_Jit());
		if (array == null)
		{
			return false;
		}
		_localizePacker = BuildTools_for_Localize.Deserialize(array);
		return true;
	}

	private MapFromCsv LoadFromCsv()
	{
		JitOption.JitLog("[[[ --- Read Enum Redirection Csv Files --- ]]]");
		return ParseCsv(File.ReadAllText(GetCsvFilePath()));
	}

	private void SerializeJit(MapFromCsv mapFromCsv)
	{
		List<string> list = new List<string>();
		list.Add(GetCsvFilePath());
		_localizePacker = BuildTools_for_Localize.Serialize(mapFromCsv, list, _loadDefine.GetFullPath_for_Jit());
	}

	public string GetCsvFilePath()
	{
		return _loadDefine.GetFullPath_for_Csv("Localization_EnumRedirection.csv");
	}

	public static MapFromCsv ParseCsv(string contents)
	{
		if (string.IsNullOrEmpty(contents))
		{
			return null;
		}
		return ParseCsv(contents.Split(new char[1] { '\n' }));
	}

	public static MapFromCsv ParseCsv(string[] lines)
	{
		if (lines == null || lines.Length == 0)
		{
			return null;
		}
		MapFromCsv mapFromCsv = new MapFromCsv();
		int num = 0;
		while (lines != null && num < lines.Length)
		{
			string text = lines[num];
			int num2 = text.IndexOf(',');
			if (num2 >= 0)
			{
				string text2 = text.Substring(0, num2);
				if (!string.IsNullOrEmpty(text2))
				{
					string text3 = text.Substring(num2 + 1);
					if (!string.IsNullOrEmpty(text3))
					{
						text3 = text3.Trim();
						text3 = text3.Trim('"');
						if (!string.IsNullOrEmpty(text3))
						{
							mapFromCsv.Remove(text2);
							mapFromCsv.Add(text2, text3);
						}
					}
				}
			}
			num++;
		}
		return mapFromCsv;
	}
}
