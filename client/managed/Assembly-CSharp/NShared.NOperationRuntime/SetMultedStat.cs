using NVespa;

namespace NShared.NOperationRuntime;

public abstract class SetMultedStat : SetStatBase
{
	public SetMultedStat()
	{
	}

	public abstract StatType[] GetMultedStatTypes();

	public override StatType[] GetStatTypes()
	{
		return GetMultedStatTypes();
	}

	public override int GetMultedStat()
	{
		if (base.OperationInfo == null)
		{
			VespaDebug.LogError("SetMultedStat - OperationInfo is  null.");
			return 0;
		}
		int num = ((base.OperationInfo.StackCount <= 0) ? 1 : base.OperationInfo.StackCount);
		return _statHolder.Get() * num;
	}
}
