namespace NShared;

[JsonSerializable(Javascript = true)]
public class EclipseBattleInfo
{
	public int MatchIndex { get; set; }

	public EclipseDeckData[] DeckList { get; set; }

	public EclipseDungeonData[] DungeonList { get; set; }

	public int AdminLevel { get; set; }

	public GuildSkillDataBase[] GuildSkills { get; set; }

	public AccountBuffDataBase[] AccountBuffs { get; set; }

	public ClassBuffDataBase[] ClassBuffDataBases { get; set; }

	public int CurrentDeckIndex { get; set; }

	public int CurrentChapterIndex { get; set; }

	public int CurrentDungeonIndex { get; set; }

	public int CurrentWaveIndex { get; set; }

	public int MaxWaveIndex { get; set; }

	public int CurrentCreatureLevel { get; set; }

	public string GameServerSessionKey { get; set; }

	public int OnlineGameSpeedRatio { get; set; }

	public DefaultExtraStatData[] ExtraStatDataBases { get; set; }

	public PetStatDataBase[] PetStatDataBases { get; set; }
}
