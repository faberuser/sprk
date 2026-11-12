using System.Collections.Generic;
using NGame2.NGuild;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class GuildRaidDungeonInfo : RaidInfoBase, IInspectorGuiDrawer
{
	public UISprite HPSprite;

	public UILabel HPValueLabel;

	public const string PrefabPath = "Resources/Prefab2/Window/GuildRaid/GuildRaidDungeonInfo";

	public UIScrollGrid KillRewardItemGrid;

	private GuildRaidDungeonData _guildRaidDungeonData;

	private CreatureData _bossCreatureData;

	public GuildRaidDungeonData GuildRaidDungeonData => _guildRaidDungeonData;

	public void Open(Listener listener, GuildRaidDungeonData data, CreatureData bossCreatureData)
	{
		_guildRaidDungeonData = data;
		_bossCreatureData = bossCreatureData;
		Open(listener, data);
		SetupMonsterHp();
		SetupKillReward();
	}

	public override string GetRaidName()
	{
		return _bossCreatureData.Name;
	}

	public override string GetMonsterTribe()
	{
		return _bossCreatureData.Tribe;
	}

	public override int GetRewardIndex()
	{
		return _guildRaidDungeonData.BuyableRewardIndex;
	}

	public override PrefabAuto<RaidSkillComponent> GetSkillComponentPrefab()
	{
		return RaidSkillComponent.GuildRaidPrefabAuto;
	}

	private void SetupMonsterHp()
	{
		int chapterIndex = _guildRaidDungeonData.ChapterIndex;
		int dungeonIndex = _guildRaidDungeonData.DungeonIndex;
		DungeonDifficultyType difficulty = _guildRaidDungeonData.Difficulty;
		NShared.GuildRaidDungeonInfo guildRaidDungeonInfo = (GlobalAutoBehaviour<GuildManager>.isValidInstance ? GlobalAutoBehaviour<GuildManager>.instance.GetGuildRaidDungeonInfo(chapterIndex) : null);
		int num = guildRaidDungeonInfo?.DungeonIndex ?? 1;
		List<MonsterHeroInfo> monsterInfoFromDungeon = UIHelper.GetMonsterInfoFromDungeon(chapterIndex, dungeonIndex, difficulty, isScenario: false, showDuplicated: true);
		long totalMaxHp = 0L;
		long totalRemainHp = 0L;
		long bossMaxHp = 0L;
		long bossRemainHp = 0L;
		GuildManager.GetGuildRaidHpInfo(monsterInfoFromDungeon, guildRaidDungeonInfo, out totalMaxHp, out totalRemainHp, out bossMaxHp, out bossRemainHp);
		if (_guildRaidDungeonData.DungeonIndex == num)
		{
			UIHelper.SetProgressValue(HPSprite, HPValueLabel, totalRemainHp, totalMaxHp);
		}
		else
		{
			UIHelper.SetProgressValue(HPSprite, HPValueLabel, totalMaxHp, totalMaxHp);
		}
	}

	private void SetupKillReward()
	{
		RewardData data = GameRoot.Instance.RewardDataContainer.GetData(_guildRaidDungeonData.KillRewardIndex);
		if (data != null)
		{
			CampaignDungeonData data2 = GameRoot.Instance.CampaignDungeonDataContainer.GetData(_dungeonData.ChapterIndex, _dungeonData.DungeonIndex);
			List<Component> collection = BaseDungeonInfo.AddRewardItemToGrid(data, KillRewardItemGrid, includeCurrency: true, null, data2.BattleType);
			_itemIconList.AddRange(collection);
		}
	}

	public override RaidType GetRaidType()
	{
		return RaidType.NONE;
	}
}
