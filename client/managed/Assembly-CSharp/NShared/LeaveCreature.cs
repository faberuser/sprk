namespace NShared;

[BattleCallback]
public sealed class LeaveCreature : BaseBattleCallback
{
	public int CreatureUid { get; set; }

	public override bool IsEquals(BaseBattleCallback callbackBase)
	{
		if (!(callbackBase is LeaveCreature leaveCreature))
		{
			return false;
		}
		if (base.GlobalTimestamp == leaveCreature.GlobalTimestamp)
		{
			return CreatureUid == leaveCreature.CreatureUid;
		}
		return false;
	}
}
