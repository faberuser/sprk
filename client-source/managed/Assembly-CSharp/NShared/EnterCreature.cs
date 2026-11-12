namespace NShared;

[BattleCallback]
public sealed class EnterCreature : BaseBattleCallback
{
	public int CreatureUid { get; set; }

	public override bool IsEquals(BaseBattleCallback callbackBase)
	{
		if (!(callbackBase is EnterCreature enterCreature))
		{
			return false;
		}
		if (base.GlobalTimestamp == enterCreature.GlobalTimestamp)
		{
			return CreatureUid == enterCreature.CreatureUid;
		}
		return false;
	}
}
