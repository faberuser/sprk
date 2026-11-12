namespace NShared;

public interface IStatObserver : ISnapshotObject
{
	void OnStatChanged(StatType statType);
}
