namespace NShared;

public class BlackBoardValueInfo : SnapshotObject
{
	private readonly SnapshotObjectMember<string> _valueHolder;

	private readonly SnapshotObjectMember<long> _addedTimeHolder;

	private readonly SnapshotObjectMember<int> _expireTimeHolder;

	public string Value
	{
		get
		{
			return _valueHolder.Get();
		}
		set
		{
			_valueHolder.Set(value);
		}
	}

	public long AddedTime
	{
		get
		{
			return _addedTimeHolder.Get();
		}
		set
		{
			_addedTimeHolder.Set(value);
		}
	}

	public int ExpireTime
	{
		get
		{
			return _expireTimeHolder.Get();
		}
		set
		{
			_expireTimeHolder.Set(value);
		}
	}

	public BlackBoardValueInfo()
	{
		_valueHolder = new SnapshotObjectMember<string>(this);
		_addedTimeHolder = new SnapshotObjectMember<long>(this, 0L);
		_expireTimeHolder = new SnapshotObjectMember<int>(this, 0);
	}
}
