namespace NShared.NOperationRuntime;

public class OverrideStat : BaseOperationRuntime, IStatOverrider, ISnapshotObject
{
	private enum FixedType
	{
		None,
		Custom,
		Base,
		Min,
		Max
	}

	protected readonly SnapshotObjectMember<StatType> _statTypeHolder;

	protected readonly SnapshotObjectMember<long> _statValueHolder;

	public OverrideStat()
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
			int valueInt2 = base.OperationInfo.GetValueInt(2);
			num = base.OperationInfo.Creature.StatController.GetFinalStat(valueEnum);
			num = ((num > valueInt2) ? num : valueInt2);
			break;
		}
		case FixedType.Max:
		{
			int valueInt = base.OperationInfo.GetValueInt(2);
			num = base.OperationInfo.Creature.StatController.GetFinalStat(valueEnum);
			num = ((num < valueInt) ? num : valueInt);
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
		if (!SprkIsValanceS3SpeedClamp())
		{
			base.OnBegin();
			base.OperationInfo.Creature.StatController.AddOverrider(this);
		}
	}

	protected override void OnEnd()
	{
		if (!SprkIsValanceS3SpeedClamp())
		{
			base.OnEnd();
			base.OperationInfo.Creature.StatController.RemoveOverrider(this);
		}
	}

	StatType IStatOverrider.GetStatType()
	{
		return _statTypeHolder.Get();
	}

	long IStatOverrider.GetOverridenStatValue()
	{
		return _statValueHolder.Get();
	}

	private bool SprkIsValanceS3SpeedClamp()
	{
		if (base.OperationInfo.OperationData.SkillIndex == 11130 && base.OperationInfo.OperationData.OperationNo == 1 && _statTypeHolder.Get() == StatType.AttackSpeed && _statValueHolder.Get() == 1000)
		{
			return true;
		}
		return false;
	}
}
