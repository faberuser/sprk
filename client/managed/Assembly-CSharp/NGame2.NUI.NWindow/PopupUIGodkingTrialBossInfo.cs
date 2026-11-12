using System.Collections.Generic;
using System.Text;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NHelper.NItem;
using NGame2.NUI.NManager;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class PopupUIGodkingTrialBossInfo : BaseWindow
{
	public enum Mode
	{
		DungeonInfo,
		BossInfo
	}

	private enum ToggleState
	{
		MonsterInfo,
		SkillInfo,
		RewardInfo
	}

	public UILabel WindowNameLabel;

	public UILabel PrefixLabel;

	public UILabel NameLabel;

	public UILabel TacticLabel;

	public UITexture PortraitTexture;

	public UIScrollGrid MonsterSkillGrid;

	public Transform MonsterInfoRoot;

	public Transform SkillInfoRoot;

	public Transform MonsterInfoButtonSelected;

	public Transform SkillInfoButtonSelected;

	public UISimpleGrid RecommendedHeroGrid;

	public UILabel LbMosnterTribe;

	public NodeTreeUIPlayer NodeTreePlayer;

	public WindowAnimation WindowAni;

	public GuildRaidInfoSkillList SkillList;

	public GameObject DungeonInfoRoot;

	public GameObject BossInfoRoot;

	public UILabel LbHeroCondition;

	public UIScrollGrid DungeonRewardGrid;

	protected CreatureData _monsterData;

	protected CampaignWaveData _waveData;

	protected CampaignWaveData.CreatureDetailInfo _monsterWaveInfo;

	protected HeroInfo _monsterInfo;

	protected List<RaidSkillComponent> _skillComponentList = new List<RaidSkillComponent>();

	protected List<HeroPortraitComponent> _recommendedHeroComponentList = new List<HeroPortraitComponent>();

	protected List<Component> _rewardIconList = new List<Component>();

	public const string PrefabPath = "Resources/Prefab2/Window/GodkingTrial/PopupUIGodkingTrialBossInfo";

	private Mode _mode;

	private GodkingTrialGroupData _godkingGroupdata;

	private int _chapterIndex;

	private int _dungeonIndex;

	private CreatureData _bossCreatureData;

	private string _bossPrefix;

	private string _bossName;

	private string _tactic;

	private int[] _recommendedHeroIndex;

	private string _previewBg;

	public override void Reset()
	{
		ClearMonsterSkill();
		ClearRecommendedHero();
		ClearDungeonReward();
		base.Reset();
	}

	public override void OnClosing()
	{
		NodeTreePlayer.Stop();
		base.OnClosing();
	}

	public static void OpenDungeonInfo(int chapterIndex, GodkingTrialGroupData godkingTrialGroupData)
	{
		WindowPrefab<PopupUIGodkingTrialBossInfo> windowPrefab = new WindowPrefab<PopupUIGodkingTrialBossInfo>("Resources/Prefab2/Window/GodkingTrial/PopupUIGodkingTrialBossInfo", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal_DungeonInfo(chapterIndex, godkingTrialGroupData);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	public static void OpenBossInfo(int chapterIndex, int dungeonIndex, GodkingTrialGroupData godkingTrialGroupData)
	{
		WindowPrefab<PopupUIGodkingTrialBossInfo> windowPrefab = new WindowPrefab<PopupUIGodkingTrialBossInfo>("Resources/Prefab2/Window/GodkingTrial/PopupUIGodkingTrialBossInfo", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal_BossInfo(chapterIndex, dungeonIndex, godkingTrialGroupData);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	public void OpenInternal_DungeonInfo(int chapterIndex, GodkingTrialGroupData godkingTrialGroupData)
	{
		base.Open();
		ChangeMode(Mode.DungeonInfo);
		SetData_DungeonInfo(chapterIndex, godkingTrialGroupData);
		UpdateUI();
	}

	public void OpenInternal_BossInfo(int chapterIndex, int dungeonIndex, GodkingTrialGroupData godkingTrialGroupData)
	{
		base.Open();
		ChangeMode(Mode.BossInfo);
		SetData_BossInfo(chapterIndex, dungeonIndex, godkingTrialGroupData);
		UpdateUI();
	}

	public void SetData_DungeonInfo(int chapterIndex, GodkingTrialGroupData godkingTrialGroupData)
	{
		_chapterIndex = chapterIndex;
		_godkingGroupdata = godkingTrialGroupData;
		_bossPrefix = LocalizationManager.GetStringDirect(godkingTrialGroupData.BossTitleNamePrefixKey);
		_bossName = LocalizationManager.GetStringDirect(godkingTrialGroupData.DungeonNameKey);
		if (NodeTreePlayer != null)
		{
			NodeTreePlayer.SetData(godkingTrialGroupData.PreviewShadowNodeTreePath, godkingTrialGroupData.PreviewShadowNodeTreeCode);
		}
	}

	public void SetData_BossInfo(int chapterIndex, int dungeonIndex, GodkingTrialGroupData godkingTrialGroupData)
	{
		_chapterIndex = chapterIndex;
		_dungeonIndex = dungeonIndex;
		_godkingGroupdata = godkingTrialGroupData;
		_bossPrefix = LocalizationManager.GetStringDirect(_godkingGroupdata.BossTitleNamePrefixKey);
		_bossName = LocalizationManager.GetStringDirect(_godkingGroupdata.DungeonNameKey);
		_tactic = godkingTrialGroupData.Tactic;
		_previewBg = godkingTrialGroupData.PreviewBg;
		_recommendedHeroIndex = godkingTrialGroupData.RecommendedHero;
		_bossCreatureData = GameRoot.Instance.CampaignDungeonDataContainer.GetBossCreatureData(chapterIndex, dungeonIndex, DungeonDifficultyType.Easy, scenario: false, out var _, out _monsterWaveInfo, out _monsterInfo);
		if (_bossCreatureData == null)
		{
			VespaDebug.LogError("bossCreatureData not found: chapterIndex=" + chapterIndex + " dungeonIndex=" + dungeonIndex);
		}
		else if (NodeTreePlayer != null)
		{
			NodeTreePlayer.SetData(godkingTrialGroupData.PreviewNodeTreePath, godkingTrialGroupData.PreviewNodeTreeCode);
		}
	}

	private void ChangeMode(Mode mode)
	{
		_mode = mode;
		switch (_mode)
		{
		case Mode.DungeonInfo:
			UIHelper.EnableGameObject(DungeonInfoRoot, enable: true);
			UIHelper.EnableGameObject(BossInfoRoot, enable: false);
			UIHelper.EnableGameObject(LbMosnterTribe, enable: false);
			break;
		case Mode.BossInfo:
			UIHelper.EnableGameObject(DungeonInfoRoot, enable: false);
			UIHelper.EnableGameObject(BossInfoRoot, enable: true);
			UIHelper.EnableGameObject(LbMosnterTribe, enable: true);
			break;
		}
	}

	private void UpdateUI()
	{
		string dungeonName = GodkingTrialHelper.GetDungeonName(_chapterIndex);
		UIHelper.SetLabelText(WindowNameLabel, dungeonName);
		UIHelper.SetLabelText(PrefixLabel, _bossPrefix);
		UIHelper.SetLabelText(NameLabel, _bossName);
		UpdateUIBoss3dView();
		switch (_mode)
		{
		case Mode.DungeonInfo:
			UpdateHeroCondition();
			UpdateReward();
			break;
		case Mode.BossInfo:
		{
			string stringDirect = LocalizationManager.GetStringDirect(_tactic);
			SprkRaidInfoScroll.SetText(TacticLabel, stringDirect, option: false);
			UpdateUIMonsterTribe(_bossCreatureData.Tribe);
			UpdateTactic();
			UpdateUIRecommendedHeroes();
			UpdateUIMonsterSkill();
			UpdateUIButtonState(ToggleState.MonsterInfo);
			break;
		}
		}
	}

	private void ClearMonsterSkill()
	{
		FactoryBase.ReleaseAndClearList(_skillComponentList);
		if (SkillList != null)
		{
			SkillList.Clear();
		}
	}

	private void UpdateTactic()
	{
		string stringDirect = LocalizationManager.GetStringDirect(_tactic);
		SprkRaidInfoScroll.SetText(TacticLabel, stringDirect, option: false);
	}

	private void UpdateUIMonsterSkill()
	{
		if (!(SkillList == null) && _monsterInfo != null)
		{
			SkillList.Open(_monsterInfo, null);
		}
	}

	private void ClearRecommendedHero()
	{
		FactoryBase.ReleaseAndClearList(_recommendedHeroComponentList);
	}

	private void UpdateUIRecommendedHeroes()
	{
		ClearRecommendedHero();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_recommendedHeroIndex); i++)
		{
			int key = _recommendedHeroIndex[i];
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

	private void UpdateUIBoss3dView()
	{
		if (!String_Extension_Method.IsNullOrEmpty(_previewBg))
		{
			_ = _previewBg;
		}
		if (NodeTreePlayer != null)
		{
			NodeTreePlayer.PlayOnce();
		}
	}

	private void SetupMonsterPortrait()
	{
		RaidHelper.InitPortrait(this, PortraitTexture, _chapterIndex, _dungeonIndex, DungeonDifficultyType.Easy);
	}

	private void UpdateUIMonsterTribe(string tribe)
	{
		if (LbMosnterTribe != null)
		{
			UIHelper.SetLabelText(LbMosnterTribe, tribe);
		}
	}

	private void ClearDungeonReward()
	{
		FactoryBase.ReleaseAndClearList(_rewardIconList);
	}

	private void UpdateReward()
	{
		ClearDungeonReward();
		int rewardIndex = _godkingGroupdata.RewardIndex;
		List<RewardElementData> rewardItemList = GameRoot.Instance.RewardDataContainer.GetData(rewardIndex).GetRewardItemList();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(rewardItemList); i++)
		{
			RewardElementData rewardElementData = rewardItemList[i];
			ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(rewardElementData.ItemCode);
			if (dataByCode != null)
			{
				ItemIconComponent itemIconComponent = UIHelper.AddGridItem(DungeonRewardGrid, ItemIconComponent.PrefabAuto);
				itemIconComponent.Open(dataByCode);
				_rewardIconList.Add(itemIconComponent);
			}
		}
		int maxDifficulty = GameRoot.Instance.GodkingTrialDataContainer.GetMaxDifficulty(_chapterIndex);
		SelectRewardData data = GameRoot.Instance.SelectRewardDataContainer.GetData(_chapterIndex, maxDifficulty);
		if (data != null)
		{
			List<SelectRewardData.SelectRewardItemInfo> selectRewardItemList = data.GetSelectRewardItemList();
			for (int j = 0; j < __Extension_Method_ICollection__.COUNT(selectRewardItemList); j++)
			{
				SelectRewardData.SelectRewardItemInfo selectRewardItemInfo = selectRewardItemList[j];
				ItemData dataByCode2 = GameRoot.Instance.ItemDataContainer.GetDataByCode(selectRewardItemInfo.Code);
				if (dataByCode2 != null)
				{
					ItemIconComponent itemIconComponent2 = UIHelper.AddGridItem(DungeonRewardGrid, ItemIconComponent.PrefabAuto);
					itemIconComponent2.Open(dataByCode2);
					_rewardIconList.Add(itemIconComponent2);
				}
			}
		}
		DungeonRewardGrid.Grid.sorting = UIGrid.Sorting.Custom;
		DungeonRewardGrid.Grid.onCustomSort = SortItem;
		DungeonRewardGrid.RepositionGrid();
	}

	private int SortItem(Transform l, Transform r)
	{
		ItemIconComponent component = l.GetComponent<ItemIconComponent>();
		ItemIconComponent component2 = r.GetComponent<ItemIconComponent>();
		int num = ItemComparer.NullCheck(component, component2);
		if (num != 0)
		{
			return num;
		}
		ItemData itemData = component.ItemData;
		ItemData itemData2 = component2.ItemData;
		if (itemData == null || itemData2 == null)
		{
			return num;
		}
		ItemComparer.SortClass sortClass = new ItemComparer.SortClass();
		sortClass.Add(ItemComparer.Compare, itemData.Tier, itemData.Tier, isAscending: false).Add(ItemComparer.Compare_ItemGrade, itemData.Grade, itemData2.Grade, isAscending: false).Add(ItemComparer.Compare, itemData.Index, itemData2.Index, isAscending: true);
		return sortClass.Sort();
	}

	private void UpdateHeroCondition()
	{
		BanRuleData data = GameRoot.Instance.BanRuleDataContainer.GetData(_godkingGroupdata.BanRuleIndex);
		if (data == null)
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		int i = 0;
		for (int num = __Extension_Method_ICollection__.COUNT(data.AcceptedClassType); i < num; i++)
		{
			CreatureTagType creatureTagType = data.AcceptedClassType[i];
			if (creatureTagType != CreatureTagType.None)
			{
				stringBuilder.Append(LocalizationManager.GetString(creatureTagType));
				if (i + 1 < num)
				{
					stringBuilder.Append(',');
				}
			}
		}
		UIHelper.SetLabelText(LbHeroCondition, stringBuilder.ToString());
	}

	private void UpdateUIButtonState(ToggleState state)
	{
		UIHelper.EnableGameObject(MonsterInfoRoot, state == ToggleState.MonsterInfo);
		UIHelper.EnableGameObject(MonsterInfoButtonSelected, state == ToggleState.MonsterInfo);
		UIHelper.EnableGameObject(SkillInfoRoot, state == ToggleState.SkillInfo);
		UIHelper.EnableGameObject(SkillInfoButtonSelected, state == ToggleState.SkillInfo);
	}

	public void OnClickMonsterInfo()
	{
		UpdateUIButtonState(ToggleState.MonsterInfo);
	}

	public void OnClickSkillInfo()
	{
		UpdateUIButtonState(ToggleState.SkillInfo);
	}

	public void OnClickRewardInfo()
	{
		UpdateUIButtonState(ToggleState.RewardInfo);
	}
}
