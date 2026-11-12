using NGame2.NGuild;
using NGame2.NUI.NComponent2;
using NGameAsset;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow;

public class GuildSuppressBossInfo : RaidInfo, IInspectorGuiDrawer
{
	public new const string PrefabPath = "Resources/Prefab2/Window/GuildSuppress/GuildSuppressBossInfo";

	private GuildSuppressDungeonData _guildSuppressDungeonData;

	private RaidData _raidData;

	private CreatureData _bossCreatureData;

	private CampaignWaveData _bossWaveData;

	public void Open(Listener listener, GuildSuppressDungeonData guildSppressDungeonData, int level)
	{
		_guildSuppressDungeonData = guildSppressDungeonData;
		GuildSuppressInfo guildSuppressInfo = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildSuppressManager.GetGuildSuppressInfo();
		_raidData = GameRoot.Instance.RaidDataContainer.GetData(guildSuppressInfo.RaidIndex, level);
		Open(listener, _raidData);
	}

	public void Open(Listener listener, GuildSuppressDungeonData guildSppressDungeonData, int raidIndex, int level)
	{
		_guildSuppressDungeonData = guildSppressDungeonData;
		_raidData = GameRoot.Instance.RaidDataContainer.GetData(raidIndex, level);
		Open(listener, _raidData);
	}

	protected override void SetupRaidInfo()
	{
		base.SetupRaidInfo();
		_bossCreatureData = GameRoot.Instance.CampaignDungeonDataContainer.GetBossCreatureData(_raidData.ChapterIndex, _raidData.DungeonIndex, DungeonDifficultyType.Easy, scenario: false, out _bossWaveData);
		string visualInfo = ((_bossWaveData != null) ? _bossWaveData.BossVisualInfo : null);
		string creatureName = string.Empty;
		string creatureNamePrefix = string.Empty;
		GameRoot.Instance.CreatureVisualDataContainer.GetCreatureName(_bossCreatureData, visualInfo, LocalizationManager.GetStringDirect, out creatureName, out creatureNamePrefix);
		UIHelper.SetLabelText(RaidNameLabel, creatureName);
		UIHelper.SetLabelText(Lbl_ChallengePrefix, creatureNamePrefix);
		string stringDirect = LocalizationManager.GetStringDirect(_guildSuppressDungeonData.Tactic);
		SprkRaidInfoScroll.SetText(RaidTacticLabel, stringDirect, option: false);
	}

	protected override void SetupRecommendedHero()
	{
		ClearRecommendedHero();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_guildSuppressDungeonData.RecommendedHeroIndex); i++)
		{
			int key = _guildSuppressDungeonData.RecommendedHeroIndex[i];
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

	protected override void SetupMonster3dView()
	{
		string previewBg = _guildSuppressDungeonData.PreviewBg;
		Boss3dView.Open(_monsterData, _waveData, previewBg, CreatureStock.ELoadLevel.IncludeFacial);
		Boss3dView.Init();
	}

	protected override void SetupMonsterPortrait()
	{
		RaidHelper.InitPortrait(this, PortraitTexture, _raidData.ChapterIndex, _raidData.DungeonIndex, DungeonDifficultyType.Easy);
	}
}
