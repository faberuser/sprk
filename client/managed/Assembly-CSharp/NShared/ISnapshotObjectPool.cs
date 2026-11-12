namespace NShared;

public interface ISnapshotObjectPool
{
	void Reset();

	void Release(object value);

	void FlushPendingRelease();
}
