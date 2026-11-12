using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NGameAsset;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public abstract class RaidInfoBase : BaseWindow
{
	public interface Listener
	{
	}

	private enum ToggleState
	{
		MonsterInfo,
		SkillInfo,
		RewardInfo
	}

	public UILabel RaidNameLabel;

	public UILabel RaidTacticLabel;

	public UILabel MonsterLevelLabel;

	public UITexture PortraitTexture;

	public Transform MonsterInfoRoot;

	public Transform SkillInfoRoot;

	public Transform RewardInfoRoot;

	public Transform KarmaRewardInfoRoot;

	public Transform MonsterInfoButtonSelected;

	public Transform SkillInfoButtonSelected;

	public Transform RewardInfoButtonSelected;

	public Transform MonsterInfoButtonSelected2;

	public Transform SkillInfoButtonSelected2;

	public Transform RewardInfoButtonSelected2;

	public UIScrollGrid RewardItemGrid;

	public UIScrollGrid KarmaRewardItemGrid;

	public UIScrollGrid KarmaRateRewardItemGrid;

	public UISimpleGrid RecommendedHeroGrid;

	[HideInInspector]
	public BaseBoss3dView Boss3dView;

	protected Listener _listener;

	private IRaidData _raidData;

	protected CampaignDungeonData _dungeonData;

	protected CreatureData _monsterData;

	protected CampaignWaveData _waveData;

	protected CampaignWaveData.CreatureDetailInfo _monsterWaveInfo;

	protected HeroInfo _monsterInfo;

	protected List<Component> _itemIconList = new List<Component>();

	protected List<Component> _bonusItemIconList = new List<Component>();

	protected List<HeroPortraitComponent> _recommendedHeroComponentList = new List<HeroPortraitComponent>();

	public GameObject RewardButtonTab;

	public GameObject SkillButtonTab;

	public UISimpleGrid ButtonTabGrid;

	public RaidInfoSkillList SkillList;

	[SerializeField]
	private UILabel LbMosnterTribe;

	public abstract string GetRaidName();

	public abstract string GetMonsterTribe();

	public abstract int GetRewardIndex();

	public abstract RaidType GetRaidType();

	public abstract PrefabAuto<RaidSkillComponent> GetSkillComponentPrefab();

	public void Open(Listener iListener, IRaidData raidData, int waveIndex = 0)
	{
		if (CheckValidRaidData(raidData, waveIndex))
		{
			base.Open();
			Init(raidData, waveIndex);
			_listener = iListener;
		}
	}

	private bool CheckValidRaidData(IRaidData raidData, int waveIndex)
	{
		if (raidData == null)
		{
			return false;
		}
		int chapterIndex = raidData.ChapterIndex;
		int dungeonIndex = raidData.DungeonIndex;
		_dungeonData = GameRoot.Instance.CampaignDungeonDataContainer.GetData(chapterIndex, dungeonIndex);
		if (_dungeonData == null)
		{
			VespaDebug.LogError("DungeonData not found : raidData=" + raidData?.ToString() + " chapterIndex=" + chapterIndex + " dungeonIndex=" + dungeonIndex);
			return false;
		}
		DungeonDifficultyType difficulty = (DungeonDifficultyType)raidData.Difficulty;
		if (!Helper.CheckDifficulty(chapterIndex, dungeonIndex, difficulty))
		{
			difficulty = (DungeonDifficultyType)Helper.GetDifficulty(chapterIndex, dungeonIndex);
		}
		if (waveIndex > 0)
		{
			_monsterData = GameRoot.Instance.CampaignDungeonDataContainer.GetBossCreatureData(chapterIndex, dungeonIndex, difficulty, scenario: false, waveIndex, out _waveData, out _monsterWaveInfo, out _monsterInfo);
		}
		else
		{
			_monsterData = GameRoot.Instance.CampaignDungeonDataContainer.GetBossCreatureData(chapterIndex, dungeonIndex, difficulty, scenario: false, out _waveData, out _monsterWaveInfo, out _monsterInfo);
		}
		if (_monsterData == null)
		{
			VespaDebug.LogError("BossCreatureData not found : raidData=" + raidData?.ToString() + " chapterIndex=" + chapterIndex + " dungeonIndex=" + dungeonIndex);
			return false;
		}
		return true;
	}

	protected void Init(IRaidData iRaidData, int waveIndex)
	{
		if (!CheckValidRaidData(iRaidData, waveIndex))
		{
			Close();
			return;
		}
		_raidData = iRaidData;
		SetupRaidInfo();
		SetupMonsterSkill();
		SetupRecommendedHero();
		SetupMonster3dView();
		SetupDropItems();
		UpdateButtonState(ToggleState.MonsterInfo);
		SetSkillButtonTab();
	}

	public override void Reset()
	{
		base.Reset();
		ClearMonsterSkill();
		ClearMonster3dView();
		ClearDropItem();
		ClearRecommendedHero();
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		Boss3dView.Update();
	}

	private void ClearMonsterSkill()
	{
		if (SkillList != null)
		{
			SkillList.Clear();
		}
	}

	private void ClearMonster3dView()
	{
		Boss3dView.Clear();
	}

	protected void ClearDropItem()
	{
		FactoryBase.ReleaseAndClearList(_itemIconList);
		FactoryBase.ReleaseAndClearList(_bonusItemIconList);
	}

	protected void ClearRecommendedHero()
	{
		FactoryBase.ReleaseAndClearList(_recommendedHeroComponentList);
	}

	protected virtual void SetupRaidInfo()
	{
		string stringDirect = LocalizationManager.GetStringDirect(GetRaidName());
		string stringDirect2 = LocalizationManager.GetStringDirect(_raidData.Tactic);
		UIHelper.SetLabelText(RaidNameLabel, stringDirect);
		UIHelper.SetLabelText(MonsterLevelLabel, _monsterInfo.Level);
		SprkRaidInfoScroll.SetText(RaidTacticLabel, stringDirect2, option: false);
		if (LbMosnterTribe != null)
		{
			UIHelper.SetLabelText(LbMosnterTribe, GetMonsterTribe());
		}
	}

	private void SetupMonsterSkill()
	{
		if (!(SkillList == null))
		{
			SkillList.Open(_monsterInfo, _raidData);
		}
	}

	protected virtual void SetupRecommendedHero()
	{
		ClearRecommendedHero();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_raidData.RecommendedHeroIndex); i++)
		{
			int key = _raidData.RecommendedHeroIndex[i];
			CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(key);
			if (data != null)
			{
				HeroPortraitComponent heroPortraitComponent = RecommendedHeroGrid.AddGridItem(HeroPortraitComponent.SmallPrefabAuto);
				heroPortraitComponent.Open(data, null, HeroPortraitComponent.EPortraitMode.Icon);
				heroPortraitComponent.ShowCreatureName(showShadow: false);
				_recommendedHeroComponentList.Add(heroPortraitComponent);
			}
		}
		RecommendedHeroGrid.RepositionGrid();
	}

	protected virtual void SetupMonster3dView()
	{
		string previewBg = _raidData.PreviewBg;
		Boss3dView.Open(_monsterData, _waveData, previewBg, CreatureStock.ELoadLevel.IncludeFacial);
		Boss3dView.Init();
		if (_raidData is RaidData raidData && !String_Extension_Method.IsNullOrEmpty(raidData.RaidInfoCustomIdle))
		{
			Boss3dView.SetCustomIdle(DefineCreature.EMotion.NamedAction, raidData.RaidInfoCustomIdle);
		}
	}

	protected virtual void SetupMonsterPortrait()
	{
		RaidHelper.InitPortrait(this, PortraitTexture, _raidData);
	}

	private void SetupDropItems()
	{
		ClearDropItem();
		if (GetRaidType() == RaidType.KarmaRaid)
		{
			SetKarmaRaidDropItems();
		}
		else
		{
			SetRaidDropItems();
		}
	}

	private void SetRaidDropItems()
	{
		List<Component> list = null;
		int rewardIndex = GetRewardIndex();
		list = AddRewardItemToGrid(rewardIndex, RewardItemGrid, includeCurrency: false, null, _dungeonData.BattleType);
		if (list != null)
		{
			_itemIconList.AddRange(list);
		}
		List<RewardElementData> rewardItemList = GetRewardItemList(rewardIndex);
		if (_raidData is RaidData raidData)
		{
			list = AddRewardItemToGrid(raidData.IndividualReward, RewardItemGrid, includeCurrency: false, rewardItemList, _dungeonData.BattleType);
			if (list != null)
			{
				_itemIconList.AddRange(list);
			}
		}
		if (_itemIconList.Count <= 0)
		{
			SetEnableRewardTab(enable: false);
		}
		else
		{
			SetEnableRewardTab(enable: true);
		}
	}

	private void SetKarmaRaidDropItems()
	{
		List<Component> list = null;
		if (_raidData is RaidData raidData)
		{
			PunishmentGroupData karmaBossPunishmentData = GameRoot.Instance.PunishmentGroupDataContainer.GetKarmaBossPunishmentData(raidData.Index);
			if (karmaBossPunishmentData != null)
			{
				list = AddRewardItemToGrid(karmaBossPunishmentData.KarmaFixRewardIndex, KarmaRewardItemGrid, includeCurrency: false, null, _dungeonData.BattleType);
				if (list != null)
				{
					_itemIconList.AddRange(list);
				}
				list = AddRewardItemToGrid(karmaBossPunishmentData.KarmaBonusRewardIndex, KarmaRateRewardItemGrid, includeCurrency: false, null, _dungeonData.BattleType);
				if (list != null)
				{
					_bonusItemIconList.AddRange(list);
				}
			}
			SetEnableRewardTab(enable: true);
		}
		else
		{
			SetEnableRewardTab(enable: false);
		}
	}

	public static List<Component> AddRewardItemToGrid(int rewardIndex, IGrid grid, bool includeCurrency = false, List<RewardElementData> addedList = null, BattleType battleType = BattleType.None)
	{
		List<Component> result = null;
		RewardData data = GameRoot.Instance.RewardDataContainer.GetData(rewardIndex);
		if (data != null)
		{
			result = BaseDungeonInfo.AddRewardItemToGrid(data, grid, includeCurrency: false, addedList, battleType);
		}
		return result;
	}

	public static List<RewardElementData> GetRewardItemList(int rewardIndex)
	{
		return GameRoot.Instance.RewardDataContainer.GetData(rewardIndex)?.GetRewardItemList();
	}

	public void SetEnableRewardTab(bool enable)
	{
		if (RewardButtonTab != null)
		{
			UIHelper.EnableGameObject(RewardButtonTab, enable);
		}
	}

	private void UpdateButtonState(ToggleState state)
	{
		UIHelper.EnableGameObject(MonsterInfoRoot, state == ToggleState.MonsterInfo);
		UIHelper.EnableGameObject(MonsterInfoButtonSelected, state == ToggleState.MonsterInfo);
		UIHelper.EnableGameObject(MonsterInfoButtonSelected2, state == ToggleState.MonsterInfo);
		UIHelper.EnableGameObject(SkillInfoRoot, state == ToggleState.SkillInfo);
		UIHelper.EnableGameObject(SkillInfoButtonSelected, state == ToggleState.SkillInfo);
		UIHelper.EnableGameObject(SkillInfoButtonSelected2, state == ToggleState.SkillInfo);
		if (GetRaidType() == RaidType.KarmaRaid)
		{
			UIHelper.EnableGameObject(KarmaRewardInfoRoot, state == ToggleState.RewardInfo);
			UIHelper.EnableGameObject(RewardInfoRoot, enable: false);
		}
		else
		{
			UIHelper.EnableGameObject(KarmaRewardInfoRoot, enable: false);
			UIHelper.EnableGameObject(RewardInfoRoot, state == ToggleState.RewardInfo);
		}
		UIHelper.EnableGameObject(RewardInfoButtonSelected, state == ToggleState.RewardInfo);
		UIHelper.EnableGameObject(RewardInfoButtonSelected2, state == ToggleState.RewardInfo);
		SprkRaidInfoScroll.Refresh(this);
	}

	private void SetSkillButtonTab()
	{
		bool enable = true;
		if (_raidData is RaidData raidData)
		{
			enable = !raidData.HideRaidInfoSkillTab;
		}
		UIHelper.EnableGameObject(SkillButtonTab, enable);
		UIHelper.RepositionGrid(ButtonTabGrid);
	}

	public void OnClickMonsterInfo()
	{
		UpdateButtonState(ToggleState.MonsterInfo);
	}

	public void OnClickSkillInfo()
	{
		UpdateButtonState(ToggleState.SkillInfo);
	}

	public void OnClickRewardInfo()
	{
		UpdateButtonState(ToggleState.RewardInfo);
	}
}
