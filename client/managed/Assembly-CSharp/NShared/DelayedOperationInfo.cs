namespace NShared;

public class DelayedOperationInfo : SnapshotObject
{
	private readonly SnapshotObjectMember<OperationInfo> _operationInfoHolder;

	private readonly SnapshotObjectMember<int> _delayMsHolder;

	private readonly SnapshotObjectMember<int> _elapsedTimeMsHolder;

	public OperationInfo OperationInfo
	{
		get
		{
			return _operationInfoHolder.Get();
		}
		set
		{
			_operationInfoHolder.Set(value);
		}
	}

	public int DelayMs
	{
		get
		{
			return _delayMsHolder.Get();
		}
		set
		{
			_delayMsHolder.Set(value);
		}
	}

	public int ElapsedTimeMs
	{
		get
		{
			return _elapsedTimeMsHolder.Get();
		}
		set
		{
			_elapsedTimeMsHolder.Set(value);
		}
	}

	public DelayedOperationInfo()
	{
		_operationInfoHolder = new SnapshotObjectMember<OperationInfo>(this);
		_delayMsHolder = new SnapshotObjectMember<int>(this, 0);
		_elapsedTimeMsHolder = new SnapshotObjectMember<int>(this, 0);
	}

	public void Update(int deltaTimeMs)
	{
		_elapsedTimeMsHolder.Set(_elapsedTimeMsHolder.Get() + deltaTimeMs);
	}

	public bool IsReady()
	{
		return _elapsedTimeMsHolder.Get() >= _delayMsHolder.Get();
	}
}
