namespace NShared;

[JsonSerializable(Javascript = true)]
public class HeroExpResultInfo
{
	public int HeroIndex { get; set; }

	public int AddValue { get; set; }

	public int AddBoosterValue { get; set; }

	public int AddTeamLevelValue { get; set; }

	public int NewValue { get; set; }

	public short OldLevel { get; set; }

	public short NewLevel { get; set; }
}
