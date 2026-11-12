namespace NShared;

[JsonSerializable(Javascript = true)]
public class EclipseHeroConditionInfo
{
	public int HeroIndex { get; set; }

	public long Hp { get; set; }

	public int Mp { get; set; }
}
