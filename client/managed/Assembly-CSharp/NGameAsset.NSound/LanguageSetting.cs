using System.Collections.Generic;
using NVespa;
using NVespa.NGlobal;
using UnityEngine;

namespace NGameAsset.NSound;

public class LanguageSetting : SingletonAuto<LanguageSetting>
{
	private LocalStorageEnum<SystemLanguage> _baseLanguage = new LocalStorageEnum<SystemLanguage>(SystemLanguage.Unknown, typeof(LanguageSetting).FullName + "BaseLanguage");

	private Dictionary<string, LocalStorageEnum<SystemLanguage>> _creatureLanguageDic = new Dictionary<string, LocalStorageEnum<SystemLanguage>>();

	public SystemLanguage GetBaseLanguage()
	{
		return _baseLanguage.Value;
	}

	public SystemLanguage GetLanguage(string creatureCode)
	{
		LocalStorageEnum<SystemLanguage> creatureLanguageStorage = GetCreatureLanguageStorage(creatureCode);
		if (creatureLanguageStorage != null && creatureLanguageStorage.Value != SystemLanguage.Unknown)
		{
			return creatureLanguageStorage.Value;
		}
		if (_baseLanguage.Value != SystemLanguage.Unknown)
		{
			return _baseLanguage.Value;
		}
		return SingletonAuto<LocalizationManager>.instance.Language;
	}

	public SystemLanguage GetCreatureLanguage(string creatureCode)
	{
		return GetCreatureLanguageStorage(creatureCode)?.Value ?? SystemLanguage.Unknown;
	}

	private LocalStorageEnum<SystemLanguage> GetCreatureLanguageStorage(string creatureCode)
	{
		if (String_Extension_Method.IsNullOrEmpty(creatureCode))
		{
			return null;
		}
		LocalStorageEnum<SystemLanguage> localStorageEnum = Vespa_Generic_IDictionary_Extension_Method.GETVALUE(_creatureLanguageDic, creatureCode, null);
		if (localStorageEnum == null)
		{
			localStorageEnum = new LocalStorageEnum<SystemLanguage>(SystemLanguage.Unknown, typeof(LanguageSetting).FullName + creatureCode);
			_creatureLanguageDic.Add(creatureCode, localStorageEnum);
		}
		return localStorageEnum;
	}

	public bool IsAllowedLanguage(SystemLanguage language)
	{
		if (language == SystemLanguage.Unknown)
		{
			return true;
		}
		return SingletonAuto<LocalizationManager>.instance.GetLanguageValue(language)?.Voice ?? false;
	}

	public void SetBaseLanguage(SystemLanguage language)
	{
		if (IsAllowedLanguage(language))
		{
			_baseLanguage.Value = language;
		}
	}

	public void SetCreatureLanguage(string creatureCode, SystemLanguage language)
	{
		if (!String_Extension_Method.IsNullOrEmpty(creatureCode) && IsAllowedLanguage(language))
		{
			LocalStorageEnum<SystemLanguage> creatureLanguageStorage = GetCreatureLanguageStorage(creatureCode);
			if (creatureLanguageStorage != null)
			{
				creatureLanguageStorage.Value = language;
			}
		}
	}
}
