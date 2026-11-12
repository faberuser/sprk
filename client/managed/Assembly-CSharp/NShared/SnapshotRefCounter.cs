namespace NShared;

public class SnapshotRefCounter
{
	private int _refCount;

	public void Reset()
	{
		_refCount = 0;
	}

	public void Increment()
	{
		_refCount++;
	}

	public int Decrement()
	{
		return --_refCount;
	}

	public bool IsAlive()
	{
		return _refCount > 0;
	}
}
