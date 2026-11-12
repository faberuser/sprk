namespace NShared;

[JsonSerializable(Javascript = true)]
public class EclipseDungeonInfo
{
	public int MatchIndex { get; set; }

	public int ChapterIndex { get; set; }

	public int DungeonIndex { get; set; }

	public int WaveIndex { get; set; }

	public EclipseStatus Status { get; set; }

	public int DeckIndex { get; set; }

	public int StartWaveIndex { get; set; }

	public int MaxWaveIndex { get; set; }

	public int CurrentCreatureLevel { get; set; }

	public string DungeonList { get; set; }

	public string GuildSkillData { get; set; }

	public string AccountBuffData { get; set; }

	public string ClassBuffData { get; set; }

	public string ExtraStatDataBases { get; set; }

	public string PetStatDataBases { get; set; }

	public int PlayTime { get; set; }

	public long TotalDamage { get; set; }

	public string UpdatedTime { get; set; }

	public string ExpireTime { get; set; }

	public string RewardedTime { get; set; }

	public int OnlineGameSpeedRatio { get; set; }

	public int EnterTicketCount { get; set; }

	public string EndWaveIndices { get; set; }
}
