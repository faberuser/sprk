using System;
using System.Linq;
using NGameAsset;
using NGameAsset.NSound;
using NVespa;
using NVespa.NGlobal;
using UnityEngine;

namespace NGame2.NScene;

public class PlayBgmInResource : MonoBehaviour
{
	[Serializable]
	public class LocalizedBgm
	{
		public SystemLanguage Language;

		public AudioClip BgmClip;
	}

	public AudioClip BgmClip;

	public float Volume;

	public LocalizedBgm[] LocalizedBgms;

	private SystemLanguage _playingBgmLanguage;

	private void Start()
	{
		SystemLanguage baseLanguage = SingletonAuto<LanguageSetting>.instance.GetBaseLanguage();
		PlayBgm(baseLanguage);
	}

	public void PlayBgm(SystemLanguage language)
	{
		if (language == SystemLanguage.Unknown)
		{
			language = SingletonAuto<LocalizationManager>.instance.Language;
			language = ((language != SystemLanguage.Unknown) ? language : Application.systemLanguage);
		}
		if (language != _playingBgmLanguage)
		{
			AudioClip bgmClip = BgmClip;
			LocalizedBgm localizedBgm = ((LocalizedBgms != null) ? LocalizedBgms.FirstOrDefault((LocalizedBgm e) => e.Language == language) : null);
			if (localizedBgm != null)
			{
				bgmClip = localizedBgm.BgmClip;
			}
			if ((bool)bgmClip)
			{
				SingletonAuto<BgmManager2>.instance.PlayFromResources(bgmClip, Volume);
			}
		}
	}
}
