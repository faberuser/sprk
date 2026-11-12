namespace NShared;

public interface ITriggerStatModifier : IStatModifier, ISnapshotObject
{
	int[] GetSkillIndex();

	SkillTriggerType GetTriggerType();

	void DecCount();
}
