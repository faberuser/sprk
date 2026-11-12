namespace NShared;

[JsonSerializable(Javascript = true)]
public class ArenaReplayInfo
{
	public int ChapterIndex { get; set; }

	public int DungeonIndex { get; set; }

	public DungeonDifficultyType Difficulty { get; set; }

	public ReplayTeamInfo LeftTeam { get; set; }

	public ReplayTeamInfo RightTeam { get; set; }

	public TeamType WinTeam { get; set; }

	public int WinMatchScore { get; set; }

	public int LoseMatchScore { get; set; }

	public int PlayTimeMs { get; set; }

	public string RecordedTime { get; set; }

	public MatchArenaType ArenaType { get; set; }
}
