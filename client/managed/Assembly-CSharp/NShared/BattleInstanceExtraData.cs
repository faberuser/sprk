namespace NShared;

[JsonSerializable(Javascript = true)]
public class BattleInstanceExtraData
{
	public int InfinityLevel { get; set; }

	public int[] AffixIndices { get; set; }

	public int FlaskItemIndex { get; set; }

	public int FlaskItemCount { get; set; }
}
