namespace NShared;

public interface ISkillTriggerInfo : ISnapshotObject
{
	SkillTriggerType TriggerType { get; }

	BattleCreature TriggerCreature { get; }

	BattleCreature[] TargetCreatures { get; }

	bool CheckTriggerValue(BattleCreature firedCreature, ITriggerValueGetter valueGetter);
}
