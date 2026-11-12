namespace NShared;

public interface IStateDurationModifier : ISnapshotObject
{
	string[] GetStateType();

	StateFlag GetStateFlag();

	int GetAddedDurationMs();

	int GetMultedDurationMs();

	ImmuneResult IsImmune();
}
