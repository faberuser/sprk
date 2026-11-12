namespace NShared;

public class SkillTickTime : SnapshotObject
{
	private readonly SnapshotObjectMember<int> _tickIndexHolder;

	private readonly SnapshotObjectMember<int> _tickMsHolder;

	private readonly SnapshotObjectMember<SnapshotList<int>> _operationNoHolder;

	public int TickIndex
	{
		get
		{
			return _tickIndexHolder.Get();
		}
		set
		{
			_tickIndexHolder.Set(value);
		}
	}

	public int TickMs
	{
		get
		{
			return _tickMsHolder.Get();
		}
		set
		{
			_tickMsHolder.Set(value);
		}
	}

	public SnapshotList<int> OperationNo
	{
		get
		{
			return _operationNoHolder.Get();
		}
		set
		{
			_operationNoHolder.Set(value);
		}
	}

	public SkillTickTime()
	{
		_tickIndexHolder = new SnapshotObjectMember<int>(this, 0);
		_tickMsHolder = new SnapshotObjectMember<int>(this, 0);
		_operationNoHolder = new SnapshotObjectMember<SnapshotList<int>>(this);
	}

	public void Init(SkillData.TickTimeData tickTimeData)
	{
		_tickIndexHolder.Set(tickTimeData.TickIndex);
		_tickMsHolder.Set(tickTimeData.TickMs);
		if (tickTimeData.OperationNo != null && tickTimeData.OperationNo.Length != 0)
		{
			_operationNoHolder.Alloc().AddRange(tickTimeData.OperationNo);
		}
	}
}
