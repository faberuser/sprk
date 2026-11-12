using NVespa;

namespace NGameAsset;

public class ConstantLockCounter : ILockCounter
{
	private int _lockCount;

	int ILockCounter.Count => _lockCount;

	int ILockCounter.UnlockTick => UnitySystemTimer.unscaledTick;

	public ConstantLockCounter(int lockCount)
	{
		_lockCount = lockCount;
	}

	void ILockCounter.Increase()
	{
	}

	void ILockCounter.Decrease(CallData lockCall)
	{
	}
}
