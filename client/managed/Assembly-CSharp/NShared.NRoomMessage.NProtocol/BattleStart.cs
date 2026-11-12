using System.Collections.Generic;

namespace NShared.NRoomMessage.NProtocol;

[JsonSerializable]
public class BattleStart
{
	public int WorldBossIndex;

	public int TowerIndex;

	public int RaidIndex;

	public int RaidLevel;

	public int Key { get; set; }

	public int TowerFloor { get; set; }

	public bool IsTowerBattle { get; set; }

	public int BattleType { get; set; }

	public BattleDefineData BattleDefine { get; set; }

	public EBattleMenu BattleMenu { get; set; }

	public string PreloadFieldname { get; set; }

	public bool PreloadDungeonMonster { get; set; }

	public List<CachedHeroInfo> PreloadCreatureInfos { get; set; }

	public int ChapterIndex { get; set; }

	public int DungeonIndex { get; set; }

	public int LinkedIndex { get; set; }

	public int WorldMapEnventIndex { get; set; }

	public int WorldMapNodeIndex { get; set; }

	public bool IsSenarioDungeon { get; set; }

	public DungeonDifficultyType DifficultType { get; set; }

	public RewardItemInfo[] DropItems { get; set; }

	public int DropGold { get; set; }

	public ResultCreatureInfo[] TowerIntialUserHeroes { get; set; }

	public ResultCreatureInfo[] TowerIntialEnemyHeroes { get; set; }

	public BaseBattleLog[] BattleLogs { get; set; }

	public MatchInfo ArenaMatchInfo { get; set; }

	public MatchSeasonData SeasonData { get; set; }

	public ArenaReplayInfo ArenaReplayInfo { get; set; }

	public CampaignDungeonData CampaignReplayInfo { get; set; }

	public ReplayTeamInfo OfflineArenaLeftTeam { get; set; }

	public MatchedAccountInfo MatchedNpcInfo { get; set; }

	public MatchedAccountInfo OfflineMyMatchInfo { get; set; }

	public string OriginalContextTypeName { get; set; }
}
