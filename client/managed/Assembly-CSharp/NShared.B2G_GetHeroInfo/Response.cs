namespace NShared.B2G_GetHeroInfo;

[BattlePacket]
public class Response : GenericResponse<ResultType>
{
	public CachedHeroInfo[] HeroInfos { get; set; }

	public CachedHeroInfo[] AiHeroInfos { get; set; }

	public CachedHeroInfo[] GroupHeroInfos { get; set; }

	public int RaidIndex { get; set; }

	public int RaidLevel { get; set; }

	public long RemainBossHp { get; set; }

	public int ChapterIndex { get; set; }

	public int DungeonIndex { get; set; }

	public int DungeonDifficulty { get; set; }

	public int AdminLevel { get; set; }

	public GuildSkillDataBase[] GuildSkills { get; set; }

	public AccountBuffDataBase[] AccountBuffs { get; set; }

	public ClassBuffDataBase[] ClassBuffDataBases { get; set; }

	public DefaultExtraStatData[] ExtraStatDataBases { get; set; }

	public PetStatDataBase[] PetStatDataBases { get; set; }
}
