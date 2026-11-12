using System.Collections.Generic;
using JsonBin;
using NVespa;
using UnityEngine;

namespace NGameAsset.NSound;

public class SoundDataContainer : SingletonAuto<SoundDataContainer>
{
	private struct LanguageSoundKey
	{
		public class Comparer : IEqualityComparer<LanguageSoundKey>
		{
			public const int MajorPrime = 7199369;

			public static Comparer Default = new Comparer();

			bool IEqualityComparer<LanguageSoundKey>.Equals(LanguageSoundKey x, LanguageSoundKey y)
			{
				if (x.Code == y.Code)
				{
					return x.Language == y.Language;
				}
				return false;
			}

			int IEqualityComparer<LanguageSoundKey>.GetHashCode(LanguageSoundKey obj)
			{
				return (int)(obj.Code.GetHashCode() * 7199369 + obj.Language);
			}
		}

		public string Code;

		public SystemLanguage Language;

		public LanguageSoundKey(string code, SystemLanguage language)
		{
			Code = code;
			Language = language;
		}
	}

	private JsonRead _rootRead;

	private Dictionary<string, SoundData> _cacheDefault = new Dictionary<string, SoundData>();

	private Dictionary<LanguageSoundKey, SoundData> _cacheByLanguage = new Dictionary<LanguageSoundKey, SoundData>(LanguageSoundKey.Comparer.Default);

	public SoundData GetSound(string code)
	{
		string creatureCode_from_SoundDefault = GetCreatureCode_from_SoundDefault(code);
		SystemLanguage language = SingletonAuto<LanguageSetting>.instance.GetLanguage(creatureCode_from_SoundDefault);
		SoundData sound_from_Language = GetSound_from_Language(language, code);
		if (sound_from_Language != null)
		{
			return sound_from_Language;
		}
		SoundData sound_from_Default = GetSound_from_Default(code);
		if (sound_from_Default == null)
		{
			return null;
		}
		return sound_from_Default;
	}

	public SoundData GetSound(SystemLanguage language, string code)
	{
		SoundData sound_from_Language = GetSound_from_Language(language, code);
		if (sound_from_Language != null)
		{
			return sound_from_Language;
		}
		SoundData sound_from_Default = GetSound_from_Default(code);
		if (sound_from_Default == null)
		{
			return null;
		}
		return sound_from_Default;
	}

	private string GetCreatureCode_from_SoundDefault(string code)
	{
		return _rootRead[code]["CreatureCode"].GETSTRING(string.Empty);
	}

	private SoundData GetSound_from_Default(string code)
	{
		if (String_Extension_Method.IsNullOrEmpty(code))
		{
			return null;
		}
		lock (_cacheDefault)
		{
			if (_cacheDefault.TryGetValue(code, out var value))
			{
				return value;
			}
			value = LoadSound_from_Default(code);
			_cacheDefault.Add(code, value);
			return value;
		}
	}

	private SoundData GetSound_from_Language(SystemLanguage language, string code)
	{
		if (String_Extension_Method.IsNullOrEmpty(code))
		{
			return null;
		}
		lock (_cacheByLanguage)
		{
			LanguageSoundKey key = new LanguageSoundKey(code, language);
			if (_cacheByLanguage.TryGetValue(key, out var value))
			{
				return value;
			}
			value = LoadSound_from_Language(language, code);
			_cacheByLanguage.Add(key, value);
			return value;
		}
	}

	private SoundData LoadSound_from_Language(SystemLanguage language, string code)
	{
		JsonRead jsonRead = _rootRead[code];
		if (!jsonRead.IsObject())
		{
			return null;
		}
		JsonRead jsonRead2 = jsonRead["Local"];
		if (!jsonRead2.IsObject())
		{
			return null;
		}
		JsonRead json = jsonRead2[language.ToString()];
		if (!json.IsObject())
		{
			return null;
		}
		SoundData soundData = new SoundData();
		soundData.LOADFROMJSON(json);
		return soundData;
	}

	private SoundData LoadSound_from_Default(string code)
	{
		JsonRead json = _rootRead[code];
		if (!json.IsObject())
		{
			return null;
		}
		SoundData soundData = new SoundData();
		soundData.LOADFROMJSON(json);
		return soundData;
	}

	public void LoadFromJson(JsonRead json)
	{
		_rootRead = json;
	}
}
