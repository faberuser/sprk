using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NGame2.NContentsDefine;
using NGame2.NUI.NComponent2;
using NShared;
using NShared.NContentsDefine;
using NVespa;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class MatchTotalInfo : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/Match/MatchTotalInfo";

	public UIToggle ToggleRules;

	public UIToggle ToggleReward;

	public GameObject RulePage;

	public GameObject RewardPage;

	public UITable RuleTable;

	public UILocalizeEx RuleSeasonDesc_Localize;

	public UITable SuddenDeathTable;

	public UIBaseGrid SuddenDeathGrid;

	public UIResizeByGrid SuddenDeathResizeGrid;

	public UIBaseGrid RewardGrid;

	private List<MatchTierRewardComponent> MatchTierRewardComponents = new List<MatchTierRewardComponent>();

	private List<SuddenDeathBuffInfoComponent> SuddenDeathComponents = new List<SuddenDeathBuffInfoComponent>();

	protected List<MatchBanPickTierRewardComponent> MatchBanPickTierRewardComponents = new List<MatchBanPickTierRewardComponent>();

	protected MatchPvpType PvpType;

	private float _returnLimitTime = 0.5f;

	private float _elapsTime = 0.5f;

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

	public override void Reset()
	{
		base.Reset();
		ClearRewardList();
		ClearSuddenDeathList();
	}

	public virtual void Init(MatchPvpType pvpType)
	{
		PvpType = pvpType;
		_elapsTime = 0f;
		InitRuleDesc();
		InitSuddenDeathInfo();
		InitSuddenDeathList();
		InitBasicRewardInfo();
		VespaCoroutine.Start(this, UpdateTableGrid(1));
	}

	public override void Close()
	{
		base.Close();
		_elapsTime = _returnLimitTime;
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		if (_elapsTime < _returnLimitTime)
		{
			_elapsTime += Time.deltaTime;
		}
	}

	private void InitRuleDesc()
	{
		NShared.NContentsDefine.MatchArena.SeasonDesc seasonDesc = ContentsDefine.MatchArena.GetSeasonDesc(PvpType.ToMatchArenaType());
		if (RuleSeasonDesc_Localize != null && seasonDesc != null)
		{
			string string_of_TimeWithDayOfWeek = NGame2.NContentsDefine.MatchArena.GetString_of_TimeWithDayOfWeek(seasonDesc.Begin);
			string string_of_TimeWithDayOfWeek2 = NGame2.NContentsDefine.MatchArena.GetString_of_TimeWithDayOfWeek(seasonDesc.End);
			RuleSeasonDesc_Localize.SetLocalizeArg(string_of_TimeWithDayOfWeek, string_of_TimeWithDayOfWeek2);
		}
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
		string curBattleDefineCode = NGame2.NContentsDefine.MatchArena.GetBattleDefineCode(PvpType.ToMatchArenaType());
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
			SuddenDeathComponents.Add(suddenDeathBuffInfoComponent);
		}
		UIHelper.RepositionGrid(SuddenDeathGrid);
	}

	protected virtual void InitBasicRewardInfo()
	{
		ClearRewardList();
		if (PvpType == MatchPvpType.BanPick)
		{
			return;
		}
		if (MatchTierRewardComponents == null)
		{
			MatchTierRewardComponents = new List<MatchTierRewardComponent>();
		}
		foreach (MatchTierType allTierType in GameRoot.Instance.MatchTierDataContainer.GetAllTierTypeList())
		{
			MatchTierRewardComponent matchTierRewardComponent = UIHelper.AddGridItem(RewardGrid, MatchTierRewardComponent.PrefabAuto);
			matchTierRewardComponent.Open(allTierType, PvpType);
			MatchTierRewardComponents.Add(matchTierRewardComponent);
		}
		MatchTierRewardComponent matchTierRewardComponent2 = UIHelper.AddGridItem(RewardGrid, MatchTierRewardComponent.PrefabAuto);
		matchTierRewardComponent2.OpenChampion(PvpType);
		MatchTierRewardComponents.Add(matchTierRewardComponent2);
		UIHelper.RepositionGrid(RewardGrid);
	}

	public virtual void OnToggleClick()
	{
		UIHelper.EnableGameObject(RulePage, ToggleRules.value);
		UIHelper.EnableGameObject(RewardPage, ToggleReward.value);
		if (ToggleRules.value)
		{
			VespaCoroutine.Start(this, UpdateTableGrid(1));
		}
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

	private void ClearRewardList()
	{
		FactoryBase.ReleaseAndClearList(MatchTierRewardComponents);
		FactoryBase.ReleaseAndClearList(MatchBanPickTierRewardComponents);
	}

	private void ClearSuddenDeathList()
	{
		FactoryBase.ReleaseAndClearList(SuddenDeathComponents);
	}
}
