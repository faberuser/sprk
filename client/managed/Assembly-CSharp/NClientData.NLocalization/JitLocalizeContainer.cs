using System.Collections.Generic;
using System.IO;
using System.Text;
using NShared.NJit;
using NShared.NLocalization;
using NVespa;

namespace NClientData.NLocalization;

public class JitLocalizeContainer
{
	private LocalizeLoadDefine _loadDefine;

	private ILocalizePacker _localizePacker;

	private LocalizeCache _localizeCache;

	public bool IsLoaded { get; private set; }

	public LanguageType Language { get; private set; }

	public JitLocalizeContainer(LanguageType language, LocalizeLoadDefine loadDefine)
	{
		Language = language;
		_loadDefine = loadDefine;
	}

	public LocalizeCache GetLocalizeCache()
	{
		lock (this)
		{
			if (_localizeCache == null)
			{
				_localizeCache = new LocalizeCache();
			}
			return _localizeCache;
		}
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
		lock (this)
		{
			string value;
			return _localizePacker.TryGet(key, out value);
		}
	}

	public bool TryGet(string key, out string value)
	{
		if (_localizePacker == null)
		{
			value = string.Empty;
			return false;
		}
		lock (this)
		{
			return _localizePacker.TryGet(key, out value);
		}
	}

	public void Reset()
	{
		_localizePacker = null;
	}

	public bool ForceRebuild()
	{
		lock (this)
		{
			CsvLoader csvLoader = LoadFromCsv();
			if (csvLoader == null)
			{
				return false;
			}
			SerializeJit(csvLoader);
			return true;
		}
	}

	public bool Test()
	{
		lock (this)
		{
			if (!LoadFromJit())
			{
				return false;
			}
			CsvLoader csvLoader = LoadFromCsv();
			if (csvLoader == null)
			{
				return false;
			}
			foreach (KeyValuePair<string, string> item in csvLoader.Dic)
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
	}

	private bool LoadInternal()
	{
		if (LoadFromJit())
		{
			return true;
		}
		if (JitOption.LoadJitOnly)
		{
			VespaDebug.LogError($"JitLocalizeContainer({Language}).LoadFromJit is Failed: Path={_loadDefine.JitKeyPath}, FullPath={_loadDefine.GetFullPath_for_Jit()}");
			return false;
		}
		CsvLoader csvLoader = LoadFromCsv();
		if (csvLoader == null)
		{
			VespaDebug.LogError($"JitLocalizeContainer({Language}).LoadFromCsv is Failed: Path={_loadDefine.JitKeyPath}");
			return false;
		}
		SerializeJit(csvLoader);
		return true;
	}

	private bool LoadFromJit()
	{
		if (ForceRebuildJit.Rebuild)
		{
			return false;
		}
		byte[] array = LoadJitBytes();
		if (array == null)
		{
			return false;
		}
		_localizePacker = BuildTools_for_Localize.Deserialize(array);
		return true;
	}

	private byte[] LoadJitBytes()
	{
		if (_loadDefine.IsFromBasic)
		{
			byte[] bytes = PreloadInAppBytes.GetBytes(_loadDefine.JitKeyPath);
			if (bytes != null)
			{
				return bytes;
			}
		}
		string fullPath_for_Jit = _loadDefine.GetFullPath_for_Jit();
		if (!File.Exists(fullPath_for_Jit))
		{
			return null;
		}
		return File.ReadAllBytes(fullPath_for_Jit);
	}

	private CsvLoader LoadFromCsv()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("[[[ --- Read Csv Files --- ]]]");
		List<string> csvFileList = GetCsvFileList();
		CsvLoader csvLoader = new CsvLoader();
		for (int i = 0; i < csvFileList.Count; i++)
		{
			string text = csvFileList[i];
			byte[] array = File.ReadAllBytes(text);
			if (array != null)
			{
				stringBuilder.AppendLine(text);
				if (csvLoader.LoadCsv(array, text) != Language)
				{
					return null;
				}
			}
		}
		return csvLoader;
	}

	private void SerializeJit(CsvLoader csvLoader)
	{
		string fullPath_for_Jit = _loadDefine.GetFullPath_for_Jit();
		_localizePacker = BuildTools_for_Localize.Serialize(csvLoader.Dic, GetCsvFileList(), fullPath_for_Jit);
	}

	private List<string> GetCsvFileList()
	{
		List<string> list = new List<string>();
		for (int i = 0; i < _loadDefine.CategoryList.Count; i++)
		{
			string text = _loadDefine.CategoryList[i];
			for (int j = 0; j < 10; j++)
			{
				string text2 = ((j != 0) ? ("Localization_" + text + j + Vespa_String_Extension_Method.GetEnumString(Language)) : ("Localization_" + text + Vespa_String_Extension_Method.GetEnumString(Language)));
				string fullPath_for_Csv = _loadDefine.GetFullPath_for_Csv(text2 + ".csv");
				if (File.Exists(fullPath_for_Csv))
				{
					list.Add(fullPath_for_Csv);
				}
			}
		}
		return list;
	}
}
