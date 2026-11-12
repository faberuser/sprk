namespace NShared;

public interface ISkillTargetFilter : ISnapshotObject
{
	void Filter(ref BattleCreatureList creatureList, SkillTargetFilterInfo filterInfo);
}
