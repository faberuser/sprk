namespace NShared.StateAction;

public class SetState : IStateAction
{
	void IStateAction.Run(BattleCreature creature, string triggerStateType, string[] values, IStateModifier stateModifier)
	{
		string valueString = ArgParser.GetValueString(values, 0);
		int valueInt = ArgParser.GetValueInt(values, 1);
		int valueInt2 = ArgParser.GetValueInt(values, 2);
		StateData data = GameRoot.Instance.StateDataContainer.GetData(valueString);
		creature.StateController.AddState(data, valueInt, null, valueInt2);
	}
}
