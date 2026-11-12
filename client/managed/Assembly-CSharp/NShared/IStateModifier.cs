namespace NShared;

public interface IStateModifier : ISnapshotObject
{
	int GetSkillIndex();

	long GetOperationUid();

	int GetOperationNo();

	StateData GetStateData();

	int GetTickedTimeMs();

	void SetTickedTimeMs(int tickedTimeMs);

	int GetStackCount();

	void SetStackCount(int stackCount);

	BattleCreature GetFiredCreature();

	bool IsValid(long mainTimeMs);

	OperationInfo GetOperationInfo();

	int GetDurationMs();

	int GetRemainDurationMs();

	new string Dump();
}
