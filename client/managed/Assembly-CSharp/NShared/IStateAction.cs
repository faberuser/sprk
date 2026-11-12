namespace NShared;

public interface IStateAction
{
	void Run(BattleCreature creature, string triggerStateType, string[] values, IStateModifier stateModifier);
}
