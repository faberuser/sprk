namespace NShared;

[BattleCallback]
public sealed class DestroyCreature : BaseBattleCallback
{
	public int CreatureUid { get; set; }

	public override bool IsEquals(BaseBattleCallback callbackBase)
	{
		if (!(callbackBase is DestroyCreature destroyCreature))
		{
			return false;
		}
		if (base.GlobalTimestamp == destroyCreature.GlobalTimestamp)
		{
			return CreatureUid == destroyCreature.CreatureUid;
		}
		return false;
	}
}
