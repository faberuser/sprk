namespace NShared.GetGuildArenaGroup;

[Response]
public class Response : GenericResponse<ResultType>
{
	public GuildArenaRankerInfo[] Rankers { get; set; }

	public GuildArenaMemberDeckInfo[] LeftMemberDeckInfos { get; set; }

	public GuildArenaMemberDeckInfo[] RightMemberDeckInfos { get; set; }

	public int MySessionScore { get; set; }

	public int[] BattledHeroIndices { get; set; }
}
