using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NGameAsset;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class WorldBossProfile : BaseWindow, IInspectorGuiDrawer
{
	private enum ToggleState
	{
		MonsterInfo,
		SkillInfo,
		RewardInfo
	}

	public UILabel WindowNameLabel;

	public UILabel NameLabel;

	public UILabel TacticLabel;

	public UILabel MonsterLevelLabel;

	public UITexture PortraitTexture;

	public UIScrollGrid MonsterSkillGrid;

	public Transform MonsterInfoRoot;

	public Transform SkillInfoRoot;

	public Transform MonsterInfoButtonSelected;

	public Transform SkillInfoButtonSelected;

	public UISimpleGrid RecommendedHeroGrid;

	[SerializeField]
	private UILabel LbMosnterTribe;

	[HideInInspector]
	public BaseBoss3dView Boss3dView;

	public GuildRaidInfoSkillList SkillList;

	protected CreatureData _monsterData;

	protected CampaignWaveData _waveData;

	protected CampaignWaveData.CreatureDetailInfo _monsterWaveInfo;

	protected HeroInfo _monsterInfo;

	protected List<RaidSkillComponent> _skillComponentList = new List<RaidSkillComponent>();

	protected List<HeroPortraitComponent> _recommendedHeroComponentList = new List<HeroPortraitComponent>();

	public const string PrefabPath = "Resources/Prefab2/Window/WorldBoss/WorldBossProfile";

	private WorldBossData _worldBossData;

	private CreatureData _bossCreatureData;

	public override void Reset()
	{
		base.Reset();
		ClearMonsterSkill();
		ClearMonster3dView();
		ClearRecommendedHero();
	}

	public void Open(WorldBossData worldBossData, CreatureData bossCreatureData)
	{
		_worldBossData = worldBossData;
		_bossCreatureData = bossCreatureData;
		int chapterIndex = _worldBossData.ChapterIndex;
		int dungeonIndex = _worldBossData.DungeonIndex;
		_monsterData = GameRoot.Instance.CampaignDungeonDataContainer.GetBossCreatureData(chapterIndex, dungeonIndex, DungeonDifficultyType.Easy, scenario: false, out _waveData, out _monsterWaveInfo, out _monsterInfo);
		if (_monsterData == null)
		{
			VespaDebug.LogError("BossCreatureData not found : WorldBossData=" + _worldBossData?.ToString() + " chapterIndex=" + chapterIndex + " dungeonIndex=" + dungeonIndex);
		}
		else
		{
			base.Open();
			UIHelper.SetLabelText(WindowNameLabel, LocalizationManager.GetStringDirect("WORLDBOSS_INFORMATION"));
			SetupInfo();
			SetupMonsterSkill();
			SetupRecommendedHero();
			SetupMonster3dView();
			UpdateButtonState(ToggleState.MonsterInfo);
			SetMonsterTribe(_bossCreatureData.Tribe);
		}
	}

	private void ClearMonster3dView()
	{
		Boss3dView.Clear();
	}

	private void ClearMonsterSkill()
	{
		FactoryBase.ReleaseAndClearList(_skillComponentList);
		if (SkillList != null)
		{
			SkillList.Clear();
		}
	}

	private void SetupInfo()
	{
		string stringDirect = LocalizationManager.GetStringDirect(_bossCreatureData.Name);
		string stringDirect2 = LocalizationManager.GetStringDirect(_worldBossData.Tactic);
		UIHelper.SetLabelText(NameLabel, stringDirect);
		UIHelper.SetLabelText(MonsterLevelLabel, _monsterInfo.Level);
		SprkRaidInfoScroll.SetText(TacticLabel, stringDirect2, option: false);
	}

	private void SetupMonsterSkill()
	{
		if (!(SkillList == null))
		{
			SkillList.Open(_monsterInfo, null);
		}
	}

	private void ClearRecommendedHero()
	{
		FactoryBase.ReleaseAndClearList(_recommendedHeroComponentList);
	}

	private void SetupRecommendedHero()
	{
		ClearRecommendedHero();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_worldBossData.RecommendedHeroIndex); i++)
		{
			int key = _worldBossData.RecommendedHeroIndex[i];
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

	private void SetupMonster3dView()
	{
		string backgroundName = ((!String_Extension_Method.IsNullOrEmpty(_worldBossData.PreviewBg)) ? _worldBossData.PreviewBg : "WorldBoss");
		Boss3dView.Open(_monsterData, _waveData, backgroundName, CreatureStock.ELoadLevel.IncludeFacial);
		Boss3dView.Init();
	}

	private void SetMonsterTribe(string tribe)
	{
		if (LbMosnterTribe != null)
		{
			UIHelper.SetLabelText(LbMosnterTribe, tribe);
		}
	}

	private void UpdateButtonState(ToggleState state)
	{
		UIHelper.EnableGameObject(MonsterInfoRoot, state == ToggleState.MonsterInfo);
		UIHelper.EnableGameObject(MonsterInfoButtonSelected, state == ToggleState.MonsterInfo);
		UIHelper.EnableGameObject(SkillInfoRoot, state == ToggleState.SkillInfo);
		UIHelper.EnableGameObject(SkillInfoButtonSelected, state == ToggleState.SkillInfo);
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
