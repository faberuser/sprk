namespace NShared.NOperationRuntime;

public class OverrideStatFirst : BaseOperationRuntime, IStatOverrider, ISnapshotObject
{
	private enum FixedType
	{
		None,
		Custom,
		Base,
		Min,
		Max,
		Calc
	}

	protected readonly SnapshotObjectMember<StatType> _statTypeHolder;

	protected readonly SnapshotObjectMember<long> _statValueHolder;

	public OverrideStatFirst()
	{
		_statTypeHolder = new SnapshotObjectMember<StatType>(this, StatType.None);
		_statValueHolder = new SnapshotObjectMember<long>(this, 0L);
	}

	protected override void OnInit()
	{
		base.OnInit();
		StatType valueEnum = base.OperationInfo.GetValueEnum<StatType>(0);
		FixedType valueEnum2 = base.OperationInfo.GetValueEnum<FixedType>(1);
		long num = 0L;
		switch (valueEnum2)
		{
		case FixedType.Base:
			num = base.OperationInfo.Creature.StatController.GetBaseStat(valueEnum);
			break;
		case FixedType.Custom:
			num = base.OperationInfo.GetValueInt(2);
			break;
		case FixedType.Min:
		{
			int valueInt4 = base.OperationInfo.GetValueInt(2);
			num = base.OperationInfo.Creature.StatController.GetFinalStat(valueEnum);
			num = ((num > valueInt4) ? num : valueInt4);
			break;
		}
		case FixedType.Max:
		{
			int valueInt3 = base.OperationInfo.GetValueInt(2);
			num = base.OperationInfo.Creature.StatController.GetFinalStat(valueEnum);
			num = ((num < valueInt3) ? num : valueInt3);
			break;
		}
		case FixedType.Calc:
		{
			num = base.OperationInfo.Creature.StatController.GetFinalStat(valueEnum);
			int valueInt = base.OperationInfo.GetValueInt(2);
			int valueInt2 = base.OperationInfo.GetValueInt(3);
			num = num * valueInt / 1000 + valueInt2;
			break;
		}
		default:
			num = base.OperationInfo.Creature.StatController.GetFinalStat(valueEnum);
			break;
		}
		_statTypeHolder.Set(valueEnum);
		_statValueHolder.Set(num);
	}

	protected override void OnBegin()
	{
		base.OnBegin();
		base.OperationInfo.Creature.StatController.AddOverriderFirst(this);
	}

	protected override void OnEnd()
	{
		base.OnEnd();
		base.OperationInfo.Creature.StatController.RemoveOverrider(this);
	}

	StatType IStatOverrider.GetStatType()
	{
		return _statTypeHolder.Get();
	}

	long IStatOverrider.GetOverridenStatValue()
	{
		return _statValueHolder.Get();
	}
}
