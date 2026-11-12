namespace NShared;

public interface IStatOverrider : ISnapshotObject
{
	StatType GetStatType();

	long GetOverridenStatValue();
}
