using System.Collections.Generic;
using NGame2.NUI.NManager;
using NGameAsset.NSound;
using NNguiExtension;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class ArchiveHeroVoicePanel : MonoBehaviour, IHeroPanel, IArchiveHeroSubPanel, ArchiveVoiceComponent.Listener
{
	public const string PrefabPath = "Resources/Prefab2/Component/Hero/ArchiveHeroVoicePanel";

	public static PrefabAuto<ArchiveHeroVoicePanel> PrefabAuto = new PrefabAuto<ArchiveHeroVoicePanel>("Resources/Prefab2/Component/Hero/ArchiveHeroVoicePanel");

	public UIScrollGrid grid;

	public UILabel LabelCVName;

	public UILabelSelector VoiceLanguage;

	public Transform CVInfoRoot;

	private List<ArchiveVoiceComponent> voiceCompnentList = new List<ArchiveVoiceComponent>();

	private SystemLanguage _currentLanguage;

	[HideInInspector]
	public CachedHeroInfo CachedHeroInfo;

	private void Init(CachedHeroInfo cachedHeroInfo)
	{
		base.gameObject.SetActive(value: true);
		Reset();
		CachedHeroInfo = cachedHeroInfo;
		SystemLanguage systemLanguage = SingletonAuto<LocalizationManager>.instance.Language;
		if (systemLanguage != SystemLanguage.Korean && systemLanguage != SystemLanguage.Japanese)
		{
			systemLanguage = SystemLanguage.English;
		}
		SelectLanguage(systemLanguage);
		SetupGrid();
		SetupCVName();
	}

	private void Reset()
	{
		ReleaseVoiceComponent();
	}

	private void ReleaseVoiceComponent()
	{
		FactoryBase.ReleaseAndClearList(voiceCompnentList);
	}

	private void SelectLanguage(SystemLanguage language)
	{
		_currentLanguage = language;
		VoiceLanguage.SelectEnum(_currentLanguage);
		NGame2.NUI.NManager.IllustratedBookManagement.SelectedLanguage = _currentLanguage;
	}

	private void SetupGrid()
	{
		ReleaseVoiceComponent();
		IEnumerator<HeroVoiceData> enumerator = GameRoot.Instance.HeroVoiceDataContainer.GetAllData().GetEnumerator();
		while (enumerator.MoveNext())
		{
			HeroVoiceData current = enumerator.Current;
			if (current != null && current.HeroIndex == CachedHeroInfo.HeroIndex)
			{
				SoundData sound = SingletonAuto<SoundDataContainer>.instance.GetSound(_currentLanguage, current.VoiceCode);
				if (sound != null && (_currentLanguage != SystemLanguage.English || sound.Language == SystemLanguage.Unknown) && (_currentLanguage != SystemLanguage.Korean || sound.Language == SystemLanguage.Korean) && (_currentLanguage != SystemLanguage.Japanese || sound.Language == SystemLanguage.Japanese))
				{
					ArchiveVoiceComponent archiveVoiceComponent = UIHelper.AddGridItem(grid, ArchiveVoiceComponent.PrefabAuto);
					archiveVoiceComponent.Open(this, CachedHeroInfo, current);
					voiceCompnentList.Add(archiveVoiceComponent);
				}
			}
		}
		UIHelper.RepositionGrid(grid);
	}

	private void SetupCVName()
	{
		HeroProfileData data = GameRoot.Instance.HeroProfileDataContainer.GetData(CachedHeroInfo.HeroIndex);
		string text = string.Empty;
		if (_currentLanguage == SystemLanguage.Korean)
		{
			text = data.CVKor;
		}
		else if (_currentLanguage == SystemLanguage.Japanese)
		{
			text = data.CVJap;
		}
		string stringDirect = LocalizationManager.GetStringDirect(text);
		UIHelper.SetLabelText(LabelCVName, stringDirect);
		UIHelper.EnableGameObject(CVInfoRoot, !String_Extension_Method.IsNullOrEmpty(text));
	}

	public void OnClickVoiceKorean()
	{
		SelectLanguage(SystemLanguage.Korean);
		SetupCVName();
		SetupGrid();
	}

	public void OnClickVoiceEnglish()
	{
		SelectLanguage(SystemLanguage.English);
		SetupCVName();
		SetupGrid();
	}

	public void OnClickVoiceJapanese()
	{
		SelectLanguage(SystemLanguage.Japanese);
		SetupCVName();
		SetupGrid();
	}

	public void Enable(bool enable, CachedHeroInfo cachedHeroInfo, IArchiveHeroSubPanelListener listener = null)
	{
		if (enable)
		{
			Init(cachedHeroInfo);
		}
		else
		{
			Close();
		}
	}

	public void Change(CachedHeroInfo cachedHeroInfo, IArchiveHeroSubPanelListener listener = null)
	{
		Init(cachedHeroInfo);
	}

	public void Close()
	{
		base.gameObject.SetActive(value: false);
		Reset();
	}
}
