namespace NGameAsset;

public interface ILockCounter
{
	int Count { get; }

	int UnlockTick { get; }

	void Increase();

	void Decrease(CallData lockCall);
}
