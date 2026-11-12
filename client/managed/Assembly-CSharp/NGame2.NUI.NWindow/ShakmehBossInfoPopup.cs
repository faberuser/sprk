using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class ShakmehBossInfoPopup : BaseWindow
{
	private enum ToggleState
	{
		MonsterInfo,
		SkillInfo
	}

	public UILabel WindowNameLabel;

	public UILabel NameLabel;

	public UILabel PrefixLabel;

	public UILabel TribeLabel;

	public UILabel TacticLabel;

	public Transform MonsterInfoRoot;

	public Transform SkillInfoRoot;

	public Transform MonsterInfoButtonSelected;

	public Transform SkillInfoButtonSelected;

	public UISimpleGrid RecommendedHeroGrid;

	public NodeTreeUIPlayer NodeTreePlayer;

	public RaidInfoSkillList SkillList;

	protected CreatureData _monsterData;

	protected CampaignWaveData _waveData;

	protected CampaignWaveData.CreatureDetailInfo _monsterWaveInfo;

	protected HeroInfo _monsterInfo;

	protected List<HeroPortraitComponent> _recommendedHeroComponentList = new List<HeroPortraitComponent>();

	public const string PrefabPath = "Resources/Prefab2/Window/ShakmehDungeon/ShakmehBossInfoPopup";

	private ShakmehBossData _shakmehBossData;

	public override void Reset()
	{
		base.Reset();
		ClearMonsterSkill();
		ClearMonster3dView();
		ClearRecommendedHero();
	}

	public override void OnClosing()
	{
		NodeTreePlayer.Stop();
		base.OnClosing();
	}

	private void ClearMonster3dView()
	{
		if (NodeTreePlayer != null)
		{
			NodeTreePlayer.Stop();
		}
	}

	private void ClearMonsterSkill()
	{
		if (SkillList != null)
		{
			SkillList.Clear();
		}
	}

	private void ClearRecommendedHero()
	{
		FactoryBase.ReleaseAndClearList(_recommendedHeroComponentList);
	}

	public void Open(int chapterIndex, int dungeonIndex, ShakmehBossData shakmehBossData)
	{
		Open(delegate
		{
			_shakmehBossData = shakmehBossData;
			_monsterData = GameRoot.Instance.CampaignDungeonDataContainer.GetBossCreatureData(chapterIndex, dungeonIndex, DungeonDifficultyType.Easy, scenario: false, out _waveData, out _monsterWaveInfo, out _monsterInfo);
			if (_monsterData != null)
			{
				Init();
			}
		}, null);
	}

	private void Init()
	{
		string stringDirect = LocalizationManager.GetStringDirect(_shakmehBossData.BossNameKey);
		UIHelper.SetLabelText(WindowNameLabel, stringDirect);
		SetupInfo();
		SetupMonsterSkill();
		SetupRecommendedHero();
		SetupMonster3dView();
		UpdateButtonState(ToggleState.MonsterInfo);
	}

	private void SetupInfo()
	{
		string stringDirect = LocalizationManager.GetStringDirect(_shakmehBossData.DungeonNameKey);
		string stringDirect2 = LocalizationManager.GetStringDirect(_shakmehBossData.DungeonPrefixKey);
		UIHelper.SetLabelText(NameLabel, stringDirect);
		UIHelper.SetLabelText(PrefixLabel, stringDirect2);
		string stringDirect3 = LocalizationManager.GetStringDirect(_shakmehBossData.TribeKey);
		string stringDirect4 = LocalizationManager.GetStringDirect(_shakmehBossData.TacticKey);
		UIHelper.SetLabelText(TribeLabel, stringDirect3);
		SprkRaidInfoScroll.SetText(TacticLabel, stringDirect4, option: false);
	}

	private void SetupMonsterSkill()
	{
		if (!(SkillList == null))
		{
			SkillList.Open(_monsterInfo, null);
		}
	}

	private void SetupRecommendedHero()
	{
		ClearRecommendedHero();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_shakmehBossData.RecommendedHero); i++)
		{
			int key = _shakmehBossData.RecommendedHero[i];
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
		if (NodeTreePlayer != null)
		{
			NodeTreePlayer.PlayOnce(_shakmehBossData.PreviewNodeTreePath, _shakmehBossData.PreviewNodeTreeCode);
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
}
