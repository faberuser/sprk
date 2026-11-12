namespace NShared.NOperationRuntime;

public class ModStateCount : BaseOperationRuntime
{
	protected override void OnBegin()
	{
		base.OnBegin();
		string valueString = base.OperationInfo.GetValueString(0);
		int valueInt = base.OperationInfo.GetValueInt(1);
		BattleCreature firedCreature = (base.OperationInfo.GetValueBool(2, defaultValue: true) ? base.OperationInfo.FiredCreature : null);
		base.OperationInfo.Creature.StateController.ChangeModifierStateCount(valueString, firedCreature, valueInt);
	}
}
