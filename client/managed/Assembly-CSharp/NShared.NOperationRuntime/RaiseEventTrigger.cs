namespace NShared.NOperationRuntime;

public class RaiseEventTrigger : BaseOperationRuntime
{
	protected override void OnBegin()
	{
		base.OnBegin();
		BattleCreature creature = base.OperationInfo.Creature;
		long uid = base.OperationInfo.Uid;
		EventTrigger eventTrigger = new EventTrigger();
		eventTrigger.CreatureUid = creature.Uid;
		eventTrigger.Values = base.OperationInfo.GetValues();
		eventTrigger.OperationUid = uid;
		creature.RaiseBattleEvent(eventTrigger);
	}
}
