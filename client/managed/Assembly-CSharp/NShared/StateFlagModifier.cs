namespace NShared;

public class StateFlagModifier : SnapshotObject
{
	private readonly SnapshotObjectMember<string> _stateHolder;

	private readonly SnapshotObjectMember<StateFlag> _flagHolder;

	private readonly SnapshotObjectMember<string> _valueHolder;

	public string State
	{
		get
		{
			return _stateHolder.Get();
		}
		set
		{
			_stateHolder.Set(value);
		}
	}

	public StateFlag Flag
	{
		get
		{
			return _flagHolder.Get();
		}
		set
		{
			_flagHolder.Set(value);
		}
	}

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

	public StateFlagModifier()
	{
		_stateHolder = new SnapshotObjectMember<string>(this);
		_flagHolder = new SnapshotObjectMember<StateFlag>(this, StateFlag.None);
		_valueHolder = new SnapshotObjectMember<string>(this);
	}
}
