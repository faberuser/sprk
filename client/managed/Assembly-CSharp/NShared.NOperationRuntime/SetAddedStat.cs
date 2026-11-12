namespace NShared.NOperationRuntime;

public abstract class SetAddedStat : SetStatBase
{
	public SetAddedStat()
	{
	}

	public abstract StatType[] GetAddedStatTypes();

	public override StatType[] GetStatTypes()
	{
		return GetAddedStatTypes();
	}

	public override int GetAddedStat()
	{
		int num = 1;
		if (base.OperationInfo != null)
		{
			num = ((base.OperationInfo.StackCount <= 0) ? 1 : base.OperationInfo.StackCount);
		}
		return _statHolder.Get() * num;
	}
}
