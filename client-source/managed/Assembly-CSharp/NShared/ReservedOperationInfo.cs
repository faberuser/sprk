using System;

namespace NShared;

public class ReservedOperationInfo : SnapshotObject
{
	private readonly SnapshotObjectMember<OperationInfo> _operationInfoHolder;

	private readonly SnapshotObjectMember<int> _fixedDelayTimeMsHolder;

	private readonly SnapshotObjectMember<int> _delayTimeMsHolder;

	private readonly SnapshotObjectMember<int> _elapsedTimeMsHolder;

	private readonly SnapshotObjectMember<bool> _fliedHolder;

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

	public int FixedDelayTimeMs
	{
		get
		{
			return _fixedDelayTimeMsHolder.Get();
		}
		set
		{
			_fixedDelayTimeMsHolder.Set(value);
		}
	}

	public int DelayTimeMs
	{
		get
		{
			return _delayTimeMsHolder.Get();
		}
		set
		{
			_delayTimeMsHolder.Set(value);
		}
	}

	public int DistanceDelayTimeMs => Math.Max(0, DelayTimeMs - FixedDelayTimeMs);

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

	public bool Flied
	{
		get
		{
			return _fliedHolder.Get();
		}
		set
		{
			_fliedHolder.Set(value);
		}
	}

	public ReservedOperationInfo()
	{
		_operationInfoHolder = new SnapshotObjectMember<OperationInfo>(this);
		_fixedDelayTimeMsHolder = new SnapshotObjectMember<int>(this, 0);
		_delayTimeMsHolder = new SnapshotObjectMember<int>(this, 0);
		_elapsedTimeMsHolder = new SnapshotObjectMember<int>(this, 0);
		_fliedHolder = new SnapshotObjectMember<bool>(this, value: false);
	}

	public void Update(int deltaTimeMs)
	{
		ElapsedTimeMs += deltaTimeMs;
	}

	public bool IsReadyToFire()
	{
		return ElapsedTimeMs >= DelayTimeMs;
	}

	public bool IsExpired()
	{
		if (OperationInfo == null)
		{
			return true;
		}
		return OperationInfo.Expired;
	}

	public bool IsReadyToFly()
	{
		return ElapsedTimeMs >= FixedDelayTimeMs;
	}
}
