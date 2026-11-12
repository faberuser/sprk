namespace NShared.StateAction;

public class ClearState : IStateAction
{
	void IStateAction.Run(BattleCreature creature, string triggerStateType, string[] values, IStateModifier stateModifier)
	{
		string valueString = ArgParser.GetValueString(values, 0);
		creature.StateController.ClearState(valueString);
	}
}
