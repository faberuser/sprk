namespace NShared;

[JsonSerializable(Javascript = true)]
public class AchievementInfo : IAccountAttrKey<int>
{
	int IAccountAttrKey<int>.Key => AchievementIndex;

	public int AchievementIndex { get; set; }

	public int LastStep { get; set; }

	public long Achievement { get; set; }

	public int ClearTeamLevel { get; set; }

	public string UpdatedTime { get; set; }
}
