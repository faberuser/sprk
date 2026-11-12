namespace NShared;

[BattleCallback]
public sealed class KarmaCardSuffle : BaseBattleCallback
{
	public int[] SuffledCardIndexes { get; set; }

	public override bool IsEquals(BaseBattleCallback callbackBase)
	{
		if (!(callbackBase is KarmaCardSuffle))
		{
			return false;
		}
		return true;
	}
}
