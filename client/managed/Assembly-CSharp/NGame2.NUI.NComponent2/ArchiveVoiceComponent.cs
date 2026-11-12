using NGame2.NUI.NManager;
using NGameAsset;
using NGameAsset.NSound;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class ArchiveVoiceComponent : LinkFactoryComponent, InitOnCreate
{
	public interface Listener
	{
	}

	public const string PrefabPath = "Prefab2/Component/IllustratedBook/ArchiveVoiceComponent";

	public static PrefabAuto<ArchiveVoiceComponent> PrefabAuto = new PrefabAuto<ArchiveVoiceComponent>("Prefab2/Component/IllustratedBook/ArchiveVoiceComponent");

	public UILabel LabelName;

	private HeroInfo _heroInfo;

	private HeroVoiceData _heroVoiceData;

	private Listener _Listener;

	public void Open(Listener listener, CachedHeroInfo heroInfo, HeroVoiceData heroVoiceData)
	{
		_Listener = listener;
		_heroInfo = heroInfo;
		_heroVoiceData = heroVoiceData;
		string stringDirect = LocalizationManager.GetStringDirect(_heroVoiceData.VoiceNameText);
		UIHelper.SetLabelText(LabelName, stringDirect);
	}

	private void Reset()
	{
	}

	void InitOnCreate.OnCreate()
	{
		Reset();
	}

	void InitOnCreate.OnRelease()
	{
		Reset();
	}

	public void OnClickPlay()
	{
		if (SingletonAuto<SoundContainer>.instance.IsEmptySoundList() && _heroInfo.CreatureData != null)
		{
			string codeName = _heroInfo.CreatureData.CodeName;
			SystemLanguage creatureLanguage = SingletonAuto<LanguageSetting>.instance.GetCreatureLanguage(codeName);
			SingletonAuto<LanguageSetting>.instance.SetCreatureLanguage(codeName, NGame2.NUI.NManager.IllustratedBookManagement.SelectedLanguage);
			SoundStock stock = new SoundStock
			{
				Code = _heroVoiceData.VoiceCode,
				LocalTimer = GlobalTimer.DefaultLocal
			};
			SingletonAuto<SoundContainer>.instance.PlayOnce(stock, 99f);
			SingletonAuto<LanguageSetting>.instance.SetCreatureLanguage(codeName, creatureLanguage);
		}
	}
}
