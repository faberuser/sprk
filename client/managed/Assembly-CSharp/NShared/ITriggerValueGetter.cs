namespace NShared;

public interface ITriggerValueGetter
{
	bool HasTriggerValue(int valueIndex);

	string GetTriggerValue(BattleCreature firedCreature, int valueIndex);
}
