namespace NShared;

[BattleCallback]
public sealed class EventTrigger : BaseBattleCallback
{
	public int CreatureUid { get; set; }

	public string[] Values { get; set; }

	public long OperationUid { get; set; }

	public override bool IsEquals(BaseBattleCallback callbackBase)
	{
		if (!(callbackBase is EventTrigger eventTrigger))
		{
			return false;
		}
		if (base.GlobalTimestamp == eventTrigger.GlobalTimestamp && CreatureUid == eventTrigger.CreatureUid && OperationUid == eventTrigger.OperationUid)
		{
			return BaseBattleCallback.ArrayIsEquals(Values, eventTrigger.Values);
		}
		return false;
	}
}
