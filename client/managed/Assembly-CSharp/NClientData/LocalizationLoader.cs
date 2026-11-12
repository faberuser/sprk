using System.Collections.Generic;
using NClientData.NLocalization;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NUtil;
using UnityEngine;

namespace NClientData;

public class LocalizationLoader : SingletonAuto<LocalizationLoader>
{
	public const int CONTAINER_INDEX_PATCH = 0;

	public const int CONTAINER_INDEX_BASIC = 1;

	public const int CONTAINER_COUNT = 2;

	private const SystemLanguage DefaultLanguage = SystemLanguage.English;

	private bool _enableLoadFromPatch;

	private LocalStorageEnum<SystemLanguage> _language;

	private JitLocalizeContainer[] _containerArray = new JitLocalizeContainer[2];

	private JitEnumRedirector _enumRedirector;

	protected override bool ThreadSafe => true;

	public SystemLanguage Language => _language.Value;

	public LocalizationLoader()
	{
		for (int i = 0; i < _containerArray.Length; i++)
		{
			_containerArray[i] = null;
		}
		SystemLanguage systemLanguage = Application.systemLanguage;
		if (!GameRoot.Instance.LocalizationValueDataContainer.IsContain((LanguageType)systemLanguage))
		{
			systemLanguage = SystemLanguage.English;
		}
		_language = new LocalStorageEnum<SystemLanguage>(systemLanguage, typeof(LocalizationManager).FullName + "_LanguageType");
		if (!GameRoot.Instance.LocalizationValueDataContainer.IsContain((LanguageType)_language.Value))
		{
			_language.Value = SystemLanguage.English;
		}
		CreateLocalizationCultureInfo(_language.Value);
		LoadAll();
	}

	public LocalizeCache GetLocalizeCache()
	{
		lock (this)
		{
			return _containerArray[0]?.GetLocalizeCache();
		}
	}

	public void SetEnableLoadFromPatch(bool enableLoad)
	{
		lock (this)
		{
			_enableLoadFromPatch = enableLoad;
			LoadFromPatch();
		}
	}

	public void ChangeLanguage(SystemLanguage language = SystemLanguage.Unknown)
	{
		lock (this)
		{
			if (_language.Value != language && language != SystemLanguage.Unknown && GameRoot.Instance.LocalizationValueDataContainer.IsContain((LanguageType)language))
			{
				_language.Value = language;
				CreateLocalizationCultureInfo(language);
				LoadAll();
			}
		}
	}

	public bool IsValidKey(string key)
	{
		lock (this)
		{
			for (int i = 0; i < _containerArray.Length; i++)
			{
				JitLocalizeContainer jitLocalizeContainer = _containerArray[i];
				if (jitLocalizeContainer != null && jitLocalizeContainer.Contains(key))
				{
					return true;
				}
			}
			return false;
		}
	}

	public bool TryGetString(string key, out string value)
	{
		lock (this)
		{
			key = ValidateKey(key);
			for (int i = 0; i < _containerArray.Length; i++)
			{
				JitLocalizeContainer jitLocalizeContainer = _containerArray[i];
				if (jitLocalizeContainer != null && jitLocalizeContainer.TryGet(key, out value))
				{
					return true;
				}
			}
			value = string.Empty;
			return false;
		}
	}

	public string ValidateKey(string key)
	{
		lock (this)
		{
			key = key.Trim();
			key = GetEnumRedirectedKey(key);
			return key;
		}
	}

	private bool CanLoadFromPatch()
	{
		return _enableLoadFromPatch;
	}

	private void CreateLocalizationCultureInfo(SystemLanguage language)
	{
		GameRoot.Instance.LocalizationValueDataContainer.CreateLocalizationCultureInfo((LanguageType)language);
	}

	private void LoadAll()
	{
		FontSelector.instance.SetLanguage(Language);
		LoadBasic();
		LoadFromPatch();
	}

	private void LoadBasic()
	{
		LocalizeLoadDefine loadDefine = LocalizeLoadDefine.CreateLoadDefine_from_Basic((LanguageType)Language);
		JitLocalizeContainer jitLocalizeContainer = new JitLocalizeContainer((LanguageType)_language.Value, loadDefine);
		jitLocalizeContainer.Load();
		_containerArray[1] = jitLocalizeContainer;
	}

	private void LoadFromPatch()
	{
		if (!CanLoadFromPatch())
		{
			_containerArray[0] = null;
			_enumRedirector = null;
			return;
		}
		LocalizeLoadDefine loadDefine = LocalizeLoadDefine.CreateLoadDefine_from_Patch((LanguageType)Language);
		JitLocalizeContainer jitLocalizeContainer = new JitLocalizeContainer((LanguageType)Language, loadDefine);
		jitLocalizeContainer.Load();
		_containerArray[0] = jitLocalizeContainer;
		LocalizeLoadDefine loadDefine2 = LocalizeLoadDefine.CreateLoadDefine_from_Patch("Localization_EnumRedirection.jit");
		_enumRedirector = new JitEnumRedirector(loadDefine2);
		_enumRedirector.Load();
	}

	private string GetEnumRedirectedKey(string enumKey)
	{
		if (string.IsNullOrEmpty(enumKey))
		{
			return string.Empty;
		}
		if (_enumRedirector == null)
		{
			return enumKey;
		}
		if (_enumRedirector.TryGet(enumKey, out var value))
		{
			return value;
		}
		return enumKey;
	}

	public static void RebuildAllJit()
	{
		BuildTools_for_Localize.RebuildAllJit();
	}

	public static List<LocalizeLoadDefine> GetLoadDefineList_from_Basic()
	{
		return BuildTools_for_Localize.GetLoadDefineList_from_Basic();
	}
}
