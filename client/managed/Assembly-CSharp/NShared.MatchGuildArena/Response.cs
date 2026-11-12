namespace NShared.MatchGuildArena;

[Response]
public class Response : GenericResponse<ResultType>
{
	public int[] HeroIndices { get; set; }

	public MatchedAccountInfo AccountInfo { get; set; }

	public MatchedAccountInfo MatchedNpcInfo { get; set; }

	public StaminaResultInfo StaminaResult { get; set; }

	public int ChapterIndex { get; set; }

	public int DungeonIndex { get; set; }

	public GuildArenaMemberScoreInfo MyScoreInfo { get; set; }

	public GuildArenaMemberScoreInfo EnemyScoreInfo { get; set; }

	public int[] BattledHeroIndices { get; set; }
}
