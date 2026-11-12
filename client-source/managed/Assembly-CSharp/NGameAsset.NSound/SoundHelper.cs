using NShared;
using NVespa;

namespace NGameAsset.NSound;

public static class SoundHelper
{
	public const string FULL_SOUND_CODE_FORMAT = "{0}.{1}{2}";

	public const string PREFIX_SOUND_CODE = "{0}.{1}";

	public const string SOUND_CODE_POSTFIX = "{0}{1}";

	public static string GetCostumeVoicePostfix(HeroInfo heroInfo)
	{
		if (heroInfo == null)
		{
			return string.Empty;
		}
		int costumeIndex = heroInfo.CostumeIndex;
		if (costumeIndex == 0)
		{
			return string.Empty;
		}
		return GetCostumeVoicePostfix(GameRoot.Instance.CostumeDataContainer.GetData(costumeIndex));
	}

	public static string GetCostumeVoicePostfix(int costumeIndex)
	{
		if (costumeIndex == 0)
		{
			return string.Empty;
		}
		return GetCostumeVoicePostfix(GameRoot.Instance.CostumeDataContainer.GetData(costumeIndex));
	}

	public static string GetCostumeVoicePostfix(CostumeData costumeData)
	{
		if (costumeData == null || costumeData.VoicePostfix == null)
		{
			return string.Empty;
		}
		return costumeData.VoicePostfix;
	}

	public static bool IsExistSoundData(string soundCode)
	{
		return SingletonAuto<SoundDataContainer>.instance.GetSound(soundCode) != null;
	}

	public static ISound GetSoundInstance(SoundStock soundStock)
	{
		return SingletonAuto<SoundContainer>.instance.CreateSoundExact(soundStock);
	}

	public static string GetSoundCode(string prefix, string heroCode, string postfix)
	{
		string text = $"{prefix}.{heroCode}{postfix}";
		if (!IsExistSoundData(text))
		{
			text = $"{prefix}.{heroCode}";
		}
		return text;
	}

	public static string GetSoundCode(string soundBody, string postfix)
	{
		string text = $"{soundBody}{postfix}";
		if (!IsExistSoundData(text))
		{
			text = soundBody;
		}
		return text;
	}

	public static ISound GetSoundInstance(string soundPrefix, string soundCodeBody, string soundPostfix, SoundStock soundStock)
	{
		soundStock.Code = $"{soundPrefix}.{soundCodeBody}{soundPostfix}";
		ISound sound = SingletonAuto<SoundContainer>.instance.CreateSoundExact(soundStock);
		if (sound == null)
		{
			soundStock.Code = $"{soundPrefix}.{soundCodeBody}";
			sound = SingletonAuto<SoundContainer>.instance.CreateSoundExact(soundStock);
		}
		return sound;
	}
}
