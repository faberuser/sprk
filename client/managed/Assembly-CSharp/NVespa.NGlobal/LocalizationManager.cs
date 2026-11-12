using System;
using System.Collections.Generic;
using NClientData;
using NShared;
using NShared.NLocalization;
using NVespa.NGlobal.NLocalization;
using UnityEngine;

namespace NVespa.NGlobal;

public class LocalizationManager : SingletonAuto<LocalizationManager>, ILocalizer
{
	protected override bool ThreadSafe => true;

	public SystemLanguage Language
	{
		get
		{
			return SingletonAuto<LocalizationLoader>.instance.Language;
		}
		set
		{
			SingletonAuto<LocalizationLoader>.instance.ChangeLanguage(value);
		}
	}

	public LocalizationManager()
	{
		SingletonAuto<LocalizationLoader>.CreateInstance();
	}

	public void SetLanguage(SystemLanguage language)
	{
		SingletonAuto<LocalizationLoader>.instance.ChangeLanguage(language);
	}

	public bool IsValidLanguage(SystemLanguage language)
	{
		if (language == SystemLanguage.Unknown)
		{
			return false;
		}
		if (!GameRoot.Instance.LocalizationValueDataContainer.IsContain((LanguageType)language))
		{
			VespaDebug.LogError("Language not exist in packaged-table : " + language);
			return false;
		}
		return true;
	}

	public IEnumerable<SystemLanguage> GetAllLanguages()
	{
		IEnumerable<LanguageType> allLanguages = GameRoot.Instance.LocalizationValueDataContainer.GetAllLanguages();
		List<SystemLanguage> list = new List<SystemLanguage>();
		foreach (LanguageType item in allLanguages)
		{
			list.Add((SystemLanguage)item);
		}
		return list;
	}

	public void ChangeLanguage(SystemLanguage language = SystemLanguage.Unknown)
	{
		SingletonAuto<LocalizationLoader>.instance.ChangeLanguage(language);
	}

	public LocalizationValueData GetLanguageValue(SystemLanguage key)
	{
		return GameRoot.Instance.LocalizationValueDataContainer.GetData((LanguageType)key);
	}

	public static bool IsValidKey(string key)
	{
		return SingletonAuto<LocalizationLoader>.instance.IsValidKey(key);
	}

	public static string ValidateKey(string key)
	{
		key = key.Trim();
		key = SingletonAuto<LocalizationLoader>.instance.ValidateKey(key);
		return key;
	}

	public static string GetStringDirect(string key)
	{
		if (string.IsNullOrEmpty(key))
		{
			return string.Empty;
		}
		return NVespa.NGlobal.LocalGetter.Remap(NVespa.NGlobal.LocalGetter.GetDirect(key));
	}

	public static bool GetStringDirect(string key, out string value)
	{
		if (string.IsNullOrEmpty(key))
		{
			value = string.Empty;
			return false;
		}
		if (!NVespa.NGlobal.LocalGetter.TryGetDirect(key, out value))
		{
			return false;
		}
		value = NVespa.NGlobal.LocalGetter.Remap(value);
		return true;
	}

	public static string GetStringDirect(string key, params object[] args)
	{
		return NVespa.NGlobal.LocalGetter.Remap(NVespa.NGlobal.LocalGetter.GetWithArgs(key, args));
	}

	public static string GetString(Enum key, params object[] args)
	{
		if (key != null && key.ToString() == "DungeonLocked")
		{
			return "You haven't unlock this raid or stage yet.";
		}
		if (key == null)
		{
			return string.Empty;
		}
		return NVespa.NGlobal.LocalGetter.Remap(NVespa.NGlobal.LocalGetter.GetEnumWithArgs(key, args));
	}

	public static string GetStringWithDefaultKey(Enum key, Enum defaultKey, params object[] args)
	{
		if (key == null)
		{
			return GetString(defaultKey, args);
		}
		return GetString(key, args);
	}

	public static string GetResultString<ResultType>(GenericResponse<ResultType> response)
	{
		if (response == null)
		{
			return "Null response : " + typeof(ResultType).FullName;
		}
		if (response.BaseResult != BaseResultType.Success)
		{
			return NVespa.NGlobal.LocalGetter.Remap(NVespa.NGlobal.LocalGetter.GetEnumWithArgs(response.BaseResult));
		}
		return NVespa.NGlobal.LocalGetter.Remap(NVespa.NGlobal.LocalGetter.GetDirect(EnumKeyCache.ConvertEnumKeyFormat(typeof(ResultType).Name, response.Result.ToString())));
	}

	string ILocalizer.GetString(Enum type, params object[] values)
	{
		return GetString(type, values);
	}

	public static string LocalizeCode(string value)
	{
		if (string.IsNullOrEmpty(value))
		{
			return string.Empty;
		}
		if (!value.StartsWith("@"))
		{
			return value;
		}
		string[] array = value.Substring(1).Split(new char[1] { ',' });
		if (array.Length == 0)
		{
			return value;
		}
		string text = array[0];
		if (string.IsNullOrEmpty(text))
		{
			return value;
		}
		List<string> list = new List<string>();
		for (int i = 1; i < array.Length; i++)
		{
			string item = LocalizeCode(array[i]);
			list.Add(item);
		}
		object[] args = list.ToArray();
		return GetStringDirect(text, args);
	}

	public static Dictionary<string, string> ParseCsv(string contents)
	{
		Dictionary<string, string> result = new Dictionary<string, string>();
		if (string.IsNullOrEmpty(contents))
		{
			return result;
		}
		return ParseCsv(contents.Split(new char[1] { '\n' }));
	}

	public static Dictionary<string, string> ParseCsv(string[] lines)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		if (lines == null || lines.Length == 0)
		{
			return dictionary;
		}
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
							dictionary.Remove(text2);
							dictionary.Add(text2, text3);
						}
					}
				}
			}
			num++;
		}
		return dictionary;
	}
}
