using System.Collections.Generic;
using NShared;
using NShared.NLocalization;
using NVespa;

namespace NClientData.NLocalization;

public class LocalizeLoadDefine
{
	public List<string> CategoryList = new List<string>();

	public string JitKeyPath;

	public bool IsFromBasic { get; private set; }

	public LocalizeLoadDefine(bool isFromBasic)
	{
		IsFromBasic = isFromBasic;
	}

	public string GetFullPath_for_Jit()
	{
		if (IsFromBasic)
		{
			return String_Extension_Method.CombinePath(GlobalPathDefine.Localization_BasicJit, JitKeyPath);
		}
		return String_Extension_Method.CombinePath(GlobalPathDefine.Localization_TableJit, JitKeyPath);
	}

	public string GetFullPath_for_Csv(string filename)
	{
		if (IsFromBasic)
		{
			return String_Extension_Method.CombinePath(String_Extension_Method.CombinePath(GlobalPathDefine.LocalizationRepository_in_Editor, "Basic"), filename);
		}
		return String_Extension_Method.CombinePath(String_Extension_Method.CombinePath(GlobalPathDefine.LocalizationRepository_in_Editor, "TableData"), filename);
	}

	public static LocalizeLoadDefine CreateLoadDefine_from_Basic(LanguageType language)
	{
		return new LocalizeLoadDefine(isFromBasic: true)
		{
			CategoryList = new List<string> { "Basic" },
			JitKeyPath = $"Localization_Basic{language}.jit"
		};
	}

	public static LocalizeLoadDefine CreateLoadDefine_from_Patch(LanguageType language)
	{
		return CreateLoadDefine_from_Patch($"LocalizationAll_{language}.jit");
	}

	public static LocalizeLoadDefine CreateLoadDefine_from_Patch(string filename)
	{
		LocalizeLoadDefine localizeLoadDefine = new LocalizeLoadDefine(isFromBasic: false);
		string[] nameArray = EnumLookup<LocalizeCategory>.NameArray;
		localizeLoadDefine.CategoryList = new List<string>();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(nameArray); i++)
		{
			localizeLoadDefine.CategoryList.Add(nameArray[i]);
		}
		localizeLoadDefine.JitKeyPath = filename;
		return localizeLoadDefine;
	}
}
