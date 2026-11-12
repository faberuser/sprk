using System;
using NVespa;

namespace NGameAsset.NSound;

public class ClipData
{
	public enum EPlayType
	{
		Once,
		Loop
	}

	public string Name;

	public bool IsVoice;

	public int Volume;

	public int[] BeginTickMs;

	public EPlayType PlayType;

	public bool IsEnableByOption()
	{
		if (IsVoice)
		{
			return SystemOptionData.Sound_EnableCharacterVoice;
		}
		return SystemOptionData.Sound_EnableGameSound;
	}

	public void Open(string name, int volume, int[] beginTickMs, EPlayType playType)
	{
		Name = name;
		Volume = volume;
		BeginTickMs = beginTickMs;
		PlayType = playType;
		IsVoice = Name.StartsWith("Voice", StringComparison.OrdinalIgnoreCase) || Name.StartsWith("Select_VOX", StringComparison.OrdinalIgnoreCase);
	}
}
