namespace NShared.BeginContent;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "contents/begin_content";

	public int ChapterIndex { get; set; }

	public int DungeonIndex { get; set; }

	public int DungeonDifficulty { get; set; }

	public int[] HeroIndices { get; set; }

	public int[] AiHeroIndices { get; set; }

	public int GroupIndex { get; set; }

	public EPunishmentDungeonType DungeonType { get; set; }

	public int FlaskItemIndex { get; set; }

	public int FlaskItemCount { get; set; }

	public int[] AffixIndices { get; set; }

	public int Level { get; set; }
}
