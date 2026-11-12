using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NGame2.NContentsDefine;
using NGame2.NContentsInfo;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class MatchBanPickRewardInfo : BaseWindow, MatchBanPickSeasonRewardComponent.Listener, InvenMultipleItemDesc.Listener
{
	public const string PrefabPath = "Resources/Prefab2/Window/Match/MatchBanPickRewardInfo";

	[Header("[Common Group]")]
	public UIToggle ToggleRules;

	public UIToggle ToggleReward;

	public GameObject RulePage;

	public GameObject RewardPage;

	public UITable RuleTable;

	public UITable SuddenDeathTable;

	public UIBaseGrid SuddenDeathGrid;

	public UIResizeByGrid SuddenDeathResizeGrid;

	public UIBaseGrid RewardGrid;

	[Header("[Free Season Group]")]
	public GameObject FreeSeasonDesc;

	[Header("[Regular Season Group]")]
	public GameObject RegularSeasonDesc;

	[Header("[Toggle Group]")]
	public UIToggle ToggleLocalTierReward;

	public UIToggle ToggleSeasonReward;

	[Header("[Page List]")]
	public GameObject TierRewardPage;

	public GameObject ServerTotalRewardPage;

	public GameObject SeasonRewardPage;

	public UIBaseGrid SeasonRewardGrid;

	public UIBaseGrid ServerTotalRankRewardGrid;

	private MatchPvpType _pvpType;

	private MatchSeasonType _seasonType;

	private List<MatchBanPickSeasonRewardComponent> _seasonRewardComponents = new List<MatchBanPickSeasonRewardComponent>();

	private List<SuddenDeathBuffInfoComponent> _suddenDeathComponents = new List<SuddenDeathBuffInfoComponent>();

	private List<MatchTierRewardComponent> _matchTierRewardComponents = new List<MatchTierRewardComponent>();

	private List<MatchBanPickTierRewardComponent> _matchBanPickTierRewardComponents = new List<MatchBanPickTierRewardComponent>();

	private float _elapsTime = 0.5f;

	private float _returnLimitTime = 0.5f;

	public void Open(MatchPvpType pvpType)
	{
		if (!(_elapsTime < _returnLimitTime))
		{
			base.Open();
			Init(pvpType);
			if ((bool)ToggleRules)
			{
				ToggleRules.value = true;
			}
		}
	}

	public void Init(MatchPvpType pvpType)
	{
		NGame2.NContentsInfo.MatchArena.MatchSeasonInfo matchSeasonInfo = SingletonAuto<NGame2.NContentsInfo.MatchArena>.instance.GetMatchSeasonInfo(pvpType);
		if (matchSeasonInfo != null)
		{
			_seasonType = matchSeasonInfo.MatchSeasonType;
		}
		_pvpType = pvpType;
		_elapsTime = 0f;
		InitSuddenDeathInfo();
		InitSuddenDeathList();
		InitBasicRewardInfo();
		VespaCoroutine.Start(this, UpdateTableGrid(1));
		InitLeagueInfo();
		InitTierRewardInfo();
		InitSeasonRewardInfo();
	}

	public override void Close()
	{
		base.Close();
		_elapsTime = _returnLimitTime;
	}

	public override void Reset()
	{
		base.Reset();
		ClearRewardList();
		ClearSuddenDeathList();
		FactoryBase.ReleaseAndClearList(_seasonRewardComponents);
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		if (_elapsTime < _returnLimitTime)
		{
			_elapsTime += Time.deltaTime;
		}
	}

	private void ClearSuddenDeathList()
	{
		FactoryBase.ReleaseAndClearList(_suddenDeathComponents);
	}

	protected void InitSuddenDeathInfo()
	{
		InitSuddenDeathList();
		SuddenDeathResizeGrid.Refresh();
		SuddenDeathTable.Reposition();
	}

	protected void InitSuddenDeathList()
	{
		ClearSuddenDeathList();
		IList<CampaignDungeonData> allDataOfBattleType = GameRoot.Instance.CampaignDungeonDataContainer.GetAllDataOfBattleType(BattleType.Arena);
		if (__Extension_Method_ICollection__.COUNT(allDataOfBattleType) == 0)
		{
			VespaDebug.LogError("DungeonData of Arena not exist");
			return;
		}
		string curBattleDefineCode = NGame2.NContentsDefine.MatchArena.GetBattleDefineCode(_pvpType.ToMatchArenaType());
		CampaignDungeonData campaignDungeonData = allDataOfBattleType.FirstOrDefault((CampaignDungeonData campaignDungeonData2) => campaignDungeonData2.BattleDefineCode.Equals(curBattleDefineCode));
		if (campaignDungeonData == null)
		{
			VespaDebug.LogError("BattleDefineCode of DungeonData not exist");
			return;
		}
		CampaignTacticData data = GameRoot.Instance.CampaignTacticDataContainer.GetData(campaignDungeonData.ChapterIndex, campaignDungeonData.DungeonIndex, 0);
		if (data == null)
		{
			VespaDebug.LogError("Tactic not found : chapterIndex=" + campaignDungeonData.ChapterIndex + " dungeonIndex=" + campaignDungeonData.DungeonIndex);
			return;
		}
		if (data.TacticType != BattleTacticType.TimerSkill)
		{
			VespaDebug.LogError("Wrong tactic type : " + data.TacticType.ToString() + " chapterIndex=" + campaignDungeonData.ChapterIndex + " dungeonIndex=" + campaignDungeonData.DungeonIndex);
			return;
		}
		string[] tacticValue = data.TacticValue;
		for (int num = 0; num < __Extension_Method_ICollection__.COUNT(tacticValue) / 2; num++)
		{
			int valueInt = ArgParser.GetValueInt(tacticValue, num * 2);
			int valueInt2 = ArgParser.GetValueInt(tacticValue, num * 2 + 1);
			SkillData data2 = GameRoot.Instance.SkillDataContainer.GetData(valueInt2);
			SuddenDeathBuffInfoComponent suddenDeathBuffInfoComponent = SuddenDeathGrid.AddGridItem(SuddenDeathBuffInfoComponent.PrefabAuto);
			suddenDeathBuffInfoComponent.Open(num, data2, valueInt);
			_suddenDeathComponents.Add(suddenDeathBuffInfoComponent);
		}
		UIHelper.RepositionGrid(SuddenDeathGrid);
	}

	protected virtual void InitBasicRewardInfo()
	{
		ClearRewardList();
		if (_pvpType == MatchPvpType.BanPick)
		{
			return;
		}
		if (_matchTierRewardComponents == null)
		{
			_matchTierRewardComponents = new List<MatchTierRewardComponent>();
		}
		foreach (MatchTierType allTierType in GameRoot.Instance.MatchTierDataContainer.GetAllTierTypeList())
		{
			MatchTierRewardComponent matchTierRewardComponent = UIHelper.AddGridItem(RewardGrid, MatchTierRewardComponent.PrefabAuto);
			matchTierRewardComponent.Open(allTierType, _pvpType);
			_matchTierRewardComponents.Add(matchTierRewardComponent);
		}
		MatchTierRewardComponent matchTierRewardComponent2 = UIHelper.AddGridItem(RewardGrid, MatchTierRewardComponent.PrefabAuto);
		matchTierRewardComponent2.OpenChampion(_pvpType);
		_matchTierRewardComponents.Add(matchTierRewardComponent2);
		UIHelper.RepositionGrid(RewardGrid);
	}

	private void InitLeagueInfo()
	{
		UIHelper.EnableGameObject(FreeSeasonDesc, _seasonType == MatchSeasonType.Free);
		UIHelper.EnableGameObject(RegularSeasonDesc, _seasonType == MatchSeasonType.Regular);
	}

	private void InitTierRewardInfo()
	{
		FactoryBase.ReleaseAndClearList(_matchBanPickTierRewardComponents);
		if (_matchBanPickTierRewardComponents == null)
		{
			_matchBanPickTierRewardComponents = new List<MatchBanPickTierRewardComponent>();
		}
		foreach (MatchTierType allTierType in GameRoot.Instance.GlobalBanPickTierDataContainer.GetAllTierTypeList())
		{
			MatchBanPickTierRewardComponent matchBanPickTierRewardComponent = UIHelper.AddGridItem(RewardGrid, MatchBanPickTierRewardComponent.PrefabAuto);
			matchBanPickTierRewardComponent.Open(allTierType, _pvpType);
			_matchBanPickTierRewardComponents.Add(matchBanPickTierRewardComponent);
		}
		MatchBanPickTierRewardComponent matchBanPickTierRewardComponent2 = UIHelper.AddGridItem(RewardGrid, MatchBanPickTierRewardComponent.PrefabAuto);
		matchBanPickTierRewardComponent2.OpenChampion(_pvpType);
		_matchBanPickTierRewardComponents.Add(matchBanPickTierRewardComponent2);
		UIHelper.RepositionGrid(RewardGrid);
	}

	protected void InitSeasonRewardInfo()
	{
		if (SeasonRewardGrid == null)
		{
			return;
		}
		if (_seasonType == MatchSeasonType.Free)
		{
			UIHelper.EnableGameObject(ToggleSeasonReward, enable: false);
			return;
		}
		IList<MatchBanPickTierRewardData> allData = GameRoot.Instance.GlobalBanPickTierRewardDataContainer.GetAllData();
		if (__Extension_Method_ICollection__.IsNullOrEmpty(allData))
		{
			return;
		}
		List<MatchBanPickTierRewardData> list = new List<MatchBanPickTierRewardData>();
		List<MatchBanPickTierRewardData> list2 = new List<MatchBanPickTierRewardData>();
		foreach (MatchBanPickTierRewardData item in allData)
		{
			if (item != null)
			{
				if (item.ShowMaxRank > -1)
				{
					list.Add(item);
				}
				else
				{
					list2.Add(item);
				}
			}
		}
		list.Sort((MatchBanPickTierRewardData a, MatchBanPickTierRewardData b) => a.ShowMaxRank.CompareTo(b.ShowMaxRank));
		list2.Sort((MatchBanPickTierRewardData a, MatchBanPickTierRewardData b) => b.TierIndex.CompareTo(a.TierIndex));
		foreach (MatchBanPickTierRewardData item2 in list)
		{
			MatchBanPickSeasonRewardComponent matchBanPickSeasonRewardComponent = UIHelper.AddGridItem(SeasonRewardGrid, MatchBanPickSeasonRewardComponent.PrefabAuto);
			_seasonRewardComponents.Add(matchBanPickSeasonRewardComponent);
			matchBanPickSeasonRewardComponent.Open(this, item2, _seasonType);
		}
		foreach (MatchBanPickTierRewardData item3 in list2)
		{
			MatchBanPickSeasonRewardComponent matchBanPickSeasonRewardComponent2 = UIHelper.AddGridItem(SeasonRewardGrid, MatchBanPickSeasonRewardComponent.PrefabAuto);
			_seasonRewardComponents.Add(matchBanPickSeasonRewardComponent2);
			matchBanPickSeasonRewardComponent2.Open(this, item3, _seasonType);
		}
		UIHelper.RepositionGrid(SeasonRewardGrid);
	}

	public void OnToggleClick()
	{
		UIHelper.EnableGameObject(RulePage, ToggleRules.value);
		UIHelper.EnableGameObject(RewardPage, ToggleReward.value);
		if (ToggleRules.value)
		{
			VespaCoroutine.Start(this, UpdateTableGrid(1));
		}
		UIHelper.EnableGameObject(TierRewardPage, ToggleLocalTierReward.value);
		UIHelper.EnableGameObject(SeasonRewardPage, ToggleSeasonReward.value);
	}

	private IEnumerator UpdateTableGrid(int frameCount)
	{
		int count = frameCount;
		while (count > 0)
		{
			count--;
			yield return null;
		}
		RuleTable.Reposition();
	}

	public void DirectToggle(UIToggle target)
	{
		ToggleRules.value = target == ToggleRules;
		ToggleReward.value = target == ToggleReward;
		ToggleSeasonReward.value = target == ToggleSeasonReward;
		ToggleLocalTierReward.value = target == ToggleLocalTierReward;
		OnToggleClick();
	}

	void MatchBanPickSeasonRewardComponent.Listener.OnClickShowMultipleDesc(List<ItemIconComponent> itemIconComp, List<CurrencyIconComponent> currencyIconComp, int selectIndex)
	{
		WindowPrefab<InvenMultipleItemDesc> windowPrefab = new WindowPrefab<InvenMultipleItemDesc>("Resources/Prefab2/Window/Inventory/InvenMultipleItemDesc", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.Open(this, LocalizationManager.GetStringDirect("GLORY_REWARD_NAME"), itemIconComp, currencyIconComp, selectIndex);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	void InvenMultipleItemDesc.Listener.OnClickDetailButton(ItemData<CostumeSelectItemData> itemData)
	{
		if (itemData != null)
		{
			MatchBanPickCostumeRewardView instance = SceneAutoBehaviour<NGame2.NUI.NManager.IllustratedBookManagement>.instance.GetInstance<MatchBanPickCostumeRewardView>();
			if (instance != null)
			{
				GlobalAutoBehaviour<PopupManegement>.instance.CloseAll();
				instance.Open(itemData);
			}
		}
	}

	private void ClearRewardList()
	{
		FactoryBase.ReleaseAndClearList(_matchTierRewardComponents);
		FactoryBase.ReleaseAndClearList(_matchBanPickTierRewardComponents);
	}
}
