using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NComponent2.NEclipse;
using NGame2.NUI.NComponent2.NItemIconSub;
using NGame2.NUI.NHelper;
using NGame2.NUI.NWindow.NEclipse;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class EclipseRewardInfo : BaseWindow, EclipseBattleRecordComponent.Listener
{
	public enum EclipseRewardTab
	{
		Reward,
		BattleRecord
	}

	public interface Listener
	{
	}

	private Listener _listener;

	public const string PrefabPath = "Resources/Prefab2/Window/SoulWeapon/EclipseRewardInfo";

	public UIButton RewardInfoButton;

	public GameObject RewardInfoButtonSelected;

	public UIButton BattleRecordButton;

	public GameObject BattleRecordButtonSelected;

	public GameObject RewardInfoRoot;

	public GameObject RewardInfoDescRoot;

	public GameObject BattleRecordRoot;

	public GameObject BattleRecordTotalRewardRoot;

	public UIScrollView RewardInfoView;

	public UIScrollGrid EclipseBattleRecordGrid;

	public UIScrollGrid BattleRecordTotalRewardGrid;

	private EclipseRewardTab _currentTab;

	private List<EclipseBattleRecordComponent> _eclipseBattleRecordComponentList = new List<EclipseBattleRecordComponent>();

	private List<ItemIconComponent> _compList = new List<ItemIconComponent>();

	public override void Reset()
	{
		base.Reset();
		ReleaseCompList();
	}

	private void ReleaseCompList()
	{
		FactoryBase.ReleaseAndClearList(_eclipseBattleRecordComponentList);
		FactoryBase.ReleaseAndClearList(_compList);
	}

	public void Open(Listener listener, Action onComplete = null)
	{
		_listener = listener;
		Open(Init, delegate
		{
			Vespa_System_Action_Extension_Method.fire(onComplete);
		});
	}

	private void Init()
	{
		InitButton();
		InitTab();
	}

	private void InitButton()
	{
		EclipseInfo eclipseInfo = GlobalAutoBehaviour<AccountManager>.instance.EclipseManager.GetEclipseInfo();
		if (eclipseInfo == null)
		{
			UIHelper.SetWidgetGrayScale(BattleRecordButton, grayScale: true);
			return;
		}
		bool flag = SweepHelper.IsPossibleSweepStep(GameRoot.Instance.CampaignDungeonDataContainer.GetData(eclipseInfo.ChapterIndex, eclipseInfo.DungeonIndex));
		bool flag2 = __Extension_Method_ICollection__.IsNullOrEmpty(GlobalAutoBehaviour<AccountManager>.instance.EclipseManager.GetSavedDeckInfos());
		UIHelper.SetWidgetGrayScale(BattleRecordButton, !flag || flag2);
	}

	private void InitTab()
	{
		_currentTab = EclipseRewardTab.Reward;
		RefreshTabInfo(_currentTab);
	}

	private void RefreshTabInfo(EclipseRewardTab tab)
	{
		bool flag = tab == EclipseRewardTab.Reward;
		UIHelper.EnableGameObject(RewardInfoButtonSelected, flag);
		UIHelper.EnableGameObject(RewardInfoDescRoot, flag);
		UIHelper.EnableGameObject(RewardInfoRoot, flag);
		UIHelper.EnableGameObject(BattleRecordButtonSelected, !flag);
		UIHelper.EnableGameObject(BattleRecordTotalRewardRoot, !flag);
		UIHelper.EnableGameObject(BattleRecordRoot, !flag);
		if (flag)
		{
			SetRewardInfo();
			return;
		}
		ReleaseCompList();
		SetBattleRecordInfo();
		SetTotalReward();
	}

	private void SetRewardInfo()
	{
		if ((bool)RewardInfoView)
		{
			RewardInfoView.ResetPosition();
		}
	}

	private void SetBattleRecordInfo()
	{
		EclipseDeckResult[] savedDeckInfos = GlobalAutoBehaviour<AccountManager>.instance.EclipseManager.GetSavedDeckInfos();
		if (__Extension_Method_ICollection__.IsNullOrEmpty(savedDeckInfos))
		{
			return;
		}
		foreach (EclipseDeckResult eclipseDeckResult in savedDeckInfos)
		{
			if (eclipseDeckResult != null)
			{
				EclipseBattleRecordComponent eclipseBattleRecordComponent = UIHelper.AddGridItem(EclipseBattleRecordGrid, EclipseBattleRecordComponent.PrefabAuto);
				if (!(eclipseBattleRecordComponent == null))
				{
					eclipseBattleRecordComponent.Open(this, eclipseDeckResult);
					_eclipseBattleRecordComponentList.Add(eclipseBattleRecordComponent);
				}
			}
		}
		UIHelper.RepositionGrid(EclipseBattleRecordGrid);
	}

	private void SetTotalReward()
	{
		EclipseDeckResult[] savedDeckInfos = GlobalAutoBehaviour<AccountManager>.instance.EclipseManager.GetSavedDeckInfos();
		if (__Extension_Method_ICollection__.IsNullOrEmpty(savedDeckInfos))
		{
			return;
		}
		int key = GlobalAutoBehaviour<AccountManager>.instance.EclipseManager.GetEclipseInfo()?.MaxWaveIndex ?? 0;
		int num = 1;
		EclipseStartData data = GameRoot.Instance.EclipseStartDataContainer.GetData(key);
		if (data != null)
		{
			num = ((data.StartWave <= 0) ? 1 : data.StartWave);
		}
		bool flag = false;
		int bestTeamClearWaveIndex = GlobalAutoBehaviour<AccountManager>.instance.EclipseManager.GetBestTeamClearWaveIndex();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(savedDeckInfos); i++)
		{
			int clearMaxWaveIndex = savedDeckInfos[i].ClearMaxWaveIndex;
			if (num <= clearMaxWaveIndex)
			{
				bool flag2 = bestTeamClearWaveIndex > 0 && clearMaxWaveIndex >= bestTeamClearWaveIndex;
				if (flag2 && !flag)
				{
					flag = true;
					List<RewardElementData> waveRangeRewardList = GameRoot.Instance.EclipseRewardDataContainer.GetWaveRangeRewardList(clearMaxWaveIndex, 1, isBestTeam: true);
					AddRewardElementComponent(waveRangeRewardList, flag2);
				}
				List<RewardElementData> waveRangeRewardList2 = GameRoot.Instance.EclipseRewardDataContainer.GetWaveRangeRewardList(clearMaxWaveIndex);
				AddRewardElementComponent(waveRangeRewardList2);
			}
		}
		UIHelper.RepositionGrid(BattleRecordTotalRewardGrid);
	}

	private void AddRewardElementComponent(List<RewardElementData> list, bool isBestTeam = false)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(list))
		{
			return;
		}
		for (int i = 0; i < list.Count; i++)
		{
			RewardElementData rewardElementData = list[i];
			if (rewardElementData == null)
			{
				continue;
			}
			HashString itemCode = rewardElementData.ItemCode;
			ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(itemCode);
			if (dataByCode == null)
			{
				continue;
			}
			int itemMax = rewardElementData.ItemMax;
			long num = itemMax;
			if (isBestTeam)
			{
				ItemIconComponent itemIconComponent = UIHelper.AddGridItem(BattleRecordTotalRewardGrid, ItemIconComponent.PrefabAuto);
				itemIconComponent.Open(dataByCode);
				itemIconComponent.SetBestTeamMark(isBestTeam: true);
				if (dataByCode.Type != ItemType.Equip)
				{
					itemIconComponent.SetItemCount(num);
				}
				_compList.Add(itemIconComponent);
				continue;
			}
			ItemIconComponent itemIconComponent2 = _compList.Find((ItemIconComponent find) => (HashString)find.ItemData.Code == itemCode && !find.IsSubComponentActive<BestTeamComponent.ComponentProxy>());
			if (itemIconComponent2 != null)
			{
				long itemCount = itemIconComponent2.ItemCount;
				itemIconComponent2.SetItemCount(checked(itemCount + num));
				continue;
			}
			itemIconComponent2 = UIHelper.AddGridItem(BattleRecordTotalRewardGrid, ItemIconComponent.PrefabAuto);
			itemIconComponent2.Open(dataByCode);
			if (dataByCode.Type != ItemType.Equip)
			{
				itemIconComponent2.SetItemCount(num);
			}
			_compList.Add(itemIconComponent2);
		}
	}

	public void OnClickRewardInfo()
	{
		if (_currentTab != EclipseRewardTab.Reward)
		{
			_currentTab = EclipseRewardTab.Reward;
			RefreshTabInfo(_currentTab);
		}
	}

	public void OnClickBattleRecord()
	{
		if (_currentTab == EclipseRewardTab.BattleRecord)
		{
			return;
		}
		EclipseInfo eclipseInfo = GlobalAutoBehaviour<AccountManager>.instance.EclipseManager.GetEclipseInfo();
		if (eclipseInfo != null)
		{
			bool num = __Extension_Method_ICollection__.IsNullOrEmpty(GlobalAutoBehaviour<AccountManager>.instance.EclipseManager.GetSavedDeckInfos());
			bool flag = SweepHelper.IsPossibleSweepStep(GameRoot.Instance.CampaignDungeonDataContainer.GetData(eclipseInfo.ChapterIndex, eclipseInfo.DungeonIndex));
			if (num || !flag)
			{
				CenterMessage.Open(LocalizationManager.GetStringDirect("SWEEP_NOTICE_ACTIVATION_ECLIPSE_CLEAR"));
				return;
			}
			if (eclipseInfo.IsPlayEclipse)
			{
				CenterMessage.Open(LocalizationManager.GetStringDirect("SWEEP_NOTICE_ACTIVATION_ECLIPSE_INBATTLE"));
				return;
			}
			_currentTab = EclipseRewardTab.BattleRecord;
			RefreshTabInfo(_currentTab);
		}
	}

	void EclipseBattleRecordComponent.Listener.ShowReward(int deckIndex, int waveIndex)
	{
		EclipseClearWaveRewardPopup.Open(deckIndex, waveIndex);
	}
}
