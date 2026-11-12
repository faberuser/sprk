namespace NShared;

public interface IHpObserver : ISnapshotObject
{
	void OnHpChanged(long oldHp, long newHp);
}
