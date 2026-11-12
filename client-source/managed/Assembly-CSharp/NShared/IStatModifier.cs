namespace NShared;

public interface IStatModifier : ISnapshotObject
{
	StatType[] GetStatTypes();

	int GetAddedStat();

	int GetMultedStat();

	long GetOperationUid();
}
