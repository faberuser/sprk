using System.Collections.Generic;
using NGame2.NAccount;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NEclipse;

public class EclipseBattleRecordComponent : UIScrollItemBehaviour
{
	public interface Listener
	{
		void ShowReward(int deckIndex, int waveIndex);
	}

	public const string PrefabPath = "Prefab2/Component/Eclipse/EclipseBattleRecordComponent";

	public static PrefabAuto<EclipseBattleRecordComponent> PrefabAuto = new PrefabAuto<EclipseBattleRecordComponent>("Prefab2/Component/Eclipse/EclipseBattleRecordComponent");

	public UISimpleGrid HeroDeckGrid;

	public UILabel DeckNameLabel;

	public UILabel ClearWaveLabel;

	public Transform BestTeamSpriteTranform;

	public Transform MVPSpriteTransform;

	private Listener _iListener;

	private EclipseDeckResult _elipseDeckResult;

	private List<HeroPortraitComponent> _heroPortraitComponentList = new List<HeroPortraitComponent>();

	public void Open(Listener iListener, EclipseDeckResult deckResult)
	{
		Init(iListener, deckResult);
	}

	private void Init(Listener iListener, EclipseDeckResult deckResult)
	{
		_iListener = iListener;
		_elipseDeckResult = deckResult;
		SetDeckInfo();
		SetHeroDeckList();
		SetBestTeamMark();
		SetBestRecord();
	}

	private void SetDeckInfo()
	{
		if (_elipseDeckResult != null)
		{
			string stringDirect = LocalizationManager.GetStringDirect("CONTENT_ECLIPSE_HERO_PARTY_TEAM_NAME", _elipseDeckResult.DeckIndex);
			UIHelper.SetLabelText(DeckNameLabel, stringDirect);
			if (GlobalAutoBehaviour<AccountManager>.isValidInstance && GlobalAutoBehaviour<AccountManager>.instance.EclipseManager.GetEclipseRewardInfo() != null)
			{
				int startWaveIndex = GlobalAutoBehaviour<AccountManager>.instance.EclipseManager.GetEclipseRewardInfo().StartWaveIndex;
				int num = _elipseDeckResult.ClearMaxWaveIndex + 1;
				UIHelper.SetLabelText(ClearWaveLabel, (startWaveIndex >= num) ? "-" : (num - 1 + " Wave"));
			}
		}
	}

	private void SetHeroDeckList()
	{
		FactoryBase.ReleaseAndClearList(_heroPortraitComponentList);
		if (_elipseDeckResult == null || !GlobalAutoBehaviour<AccountManager>.isValidInstance)
		{
			return;
		}
		List<int> heroIndexList = GlobalAutoBehaviour<AccountManager>.instance.EclipseManager.GetHeroIndexList(_elipseDeckResult.DeckIndex);
		CachedHeroInfo heroInfo = null;
		HeroPortraitComponent heroPortraitComponent = null;
		for (int i = 0; i < 4; i++)
		{
			heroPortraitComponent = UIHelper.AddGridItem(HeroDeckGrid, HeroPortraitComponent.SmallPrefabAuto);
			if (!(heroPortraitComponent == null))
			{
				heroPortraitComponent.Open(heroInfo, null, null, HeroPortraitComponent.EPortraitMode.Icon);
				heroPortraitComponent.ResetPortrait((!__Extension_Method_ICollection__.IsNullOrEmpty(heroIndexList) && heroIndexList.Count > i) ? GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(heroIndexList[i]) : null, null);
				_heroPortraitComponentList.Add(heroPortraitComponent);
			}
		}
		UIHelper.RepositionGrid(HeroDeckGrid);
	}

	private void SetBestTeamMark()
	{
		UIHelper.EnableGameObject(BestTeamSpriteTranform, enable: false);
		if (_elipseDeckResult == null || !GlobalAutoBehaviour<AccountManager>.isValidInstance)
		{
			return;
		}
		EclipseDungeonInfo eclipseRewardInfo = GlobalAutoBehaviour<AccountManager>.instance.EclipseManager.GetEclipseRewardInfo();
		if (eclipseRewardInfo == null)
		{
			return;
		}
		int startWaveIndex = eclipseRewardInfo.StartWaveIndex;
		int num = _elipseDeckResult.ClearMaxWaveIndex + 1;
		if (startWaveIndex < num && GlobalAutoBehaviour<AccountManager>.instance.EclipseManager.CheckAdvanceWave(num))
		{
			int bestTeamClearWaveIndex = GlobalAutoBehaviour<AccountManager>.instance.EclipseManager.GetBestTeamClearWaveIndex();
			if (bestTeamClearWaveIndex > 0)
			{
				UIHelper.EnableGameObject(BestTeamSpriteTranform, _elipseDeckResult.ClearMaxWaveIndex >= bestTeamClearWaveIndex);
			}
		}
	}

	private void SetBestRecord()
	{
		UIHelper.EnableGameObject(MVPSpriteTransform, enable: false);
		if (_elipseDeckResult == null || !GlobalAutoBehaviour<AccountManager>.isValidInstance)
		{
			return;
		}
		EclipseDungeonInfo eclipseRewardInfo = GlobalAutoBehaviour<AccountManager>.instance.EclipseManager.GetEclipseRewardInfo();
		if (eclipseRewardInfo == null)
		{
			return;
		}
		EclipseInfo eclipseInfo = GlobalAutoBehaviour<AccountManager>.instance.EclipseManager.GetEclipseInfo();
		if (eclipseInfo != null && eclipseRewardInfo.MaxWaveIndex > eclipseInfo.MaxWaveIndex)
		{
			int num = ((eclipseRewardInfo.MaxWaveIndex > eclipseInfo.MaxWaveIndex) ? eclipseRewardInfo.MaxWaveIndex : eclipseInfo.MaxWaveIndex);
			if (num > 0)
			{
				UIHelper.EnableGameObject(MVPSpriteTransform, num <= _elipseDeckResult.ClearMaxWaveIndex);
			}
		}
	}

	public void OnClickShowReward()
	{
		if (_iListener != null && _elipseDeckResult != null)
		{
			_iListener.ShowReward(_elipseDeckResult.DeckIndex, _elipseDeckResult.ClearMaxWaveIndex);
		}
	}
}
