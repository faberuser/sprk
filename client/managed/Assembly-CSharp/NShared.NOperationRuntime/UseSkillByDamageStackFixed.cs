using NShared.NUtil;

namespace NShared.NOperationRuntime;

public class UseSkillByDamageStackFixed : BaseOperationRuntime, IDamageModifier, IBaseDamageModifier, ISnapshotObject
{
	protected readonly SnapshotObjectMember<CheckType> _compareTypeHolder;

	protected readonly SnapshotObjectMember<long> _compareValueHolder;

	protected readonly SnapshotObjectMember<long> _damageStackHolder;

	protected readonly SnapshotObjectMember<int> _skillIndexHolder;

	protected readonly SnapshotObjectMember<OperationType> _skillUseTypeHolder;

	private CheckType CheckType
	{
		get
		{
			return _compareTypeHolder.Get();
		}
		set
		{
			_compareTypeHolder.Set(value);
		}
	}

	private long CompareStatValue
	{
		get
		{
			return _compareValueHolder.Get();
		}
		set
		{
			_compareValueHolder.Set(value);
		}
	}

	private long DamageStack
	{
		get
		{
			return _damageStackHolder.Get();
		}
		set
		{
			_damageStackHolder.Set(value);
		}
	}

	private int SkillIndex
	{
		get
		{
			return _skillIndexHolder.Get();
		}
		set
		{
			_skillIndexHolder.Set(value);
		}
	}

	private OperationType UseType
	{
		get
		{
			return _skillUseTypeHolder.Get();
		}
		set
		{
			_skillUseTypeHolder.Set(value);
		}
	}

	public UseSkillByDamageStackFixed()
	{
		_compareTypeHolder = new SnapshotObjectMember<CheckType>(this, CheckType.None);
		_compareValueHolder = new SnapshotObjectMember<long>(this, 0L);
		_damageStackHolder = new SnapshotObjectMember<long>(this, 0L);
		_skillIndexHolder = new SnapshotObjectMember<int>(this, 0);
		_skillUseTypeHolder = new SnapshotObjectMember<OperationType>(this, OperationType.None);
	}

	protected override void OnInit()
	{
		_damageStackHolder.Set(0L);
		StatType valueEnum = base.OperationInfo.GetValueEnum<StatType>(0);
		int valueInt = base.OperationInfo.GetValueInt(1);
		BattleCreature battleCreature = ((base.OperationInfo.TargetCreature != null) ? base.OperationInfo.TargetCreature : base.OperationInfo.Creature);
		if (battleCreature == null)
		{
			CompareStatValue = 0L;
		}
		else
		{
			long finalStat = battleCreature.StatController.GetFinalStat(valueEnum);
			CompareStatValue = finalStat * valueInt / 1000;
		}
		CheckType = base.OperationInfo.GetValueEnum<CheckType>(2);
		SkillIndex = base.OperationInfo.GetValueInt(3);
		UseType = base.OperationInfo.GetValueEnum<OperationType>(4);
	}

	protected override void OnBegin()
	{
		base.OnBegin();
		base.OperationInfo.Creature.StatController.AddDamageModifier(this);
	}

	protected override void OnEnd()
	{
		base.OnEnd();
		base.OperationInfo.Creature.StatController.RemoveDamageModifier(this);
	}

	bool IDamageModifier.Modify(BattleCreature attacker, ref long amount, ref DamageFlag damageFlag)
	{
		DamageStack += amount;
		if (!Condition.CheckCompareResult(CheckType, CompareStatValue, DamageStack))
		{
			return false;
		}
		SkillData data = GameRoot.Instance.SkillDataContainer.GetData(SkillIndex);
		if (data == null)
		{
			return false;
		}
		switch (UseType)
		{
		case OperationType.UseSkill:
			base.OperationInfo.Creature.SkillController.UseSkill(data, null, null);
			break;
		case OperationType.RegisterPassiveSkill:
			base.OperationInfo.Creature.SkillController.PassiveSkillContainer.Register(data, 1);
			break;
		}
		base.OperationInfo.Creature.StatController.RemoveDamageModifier(this);
		return true;
	}

	DamageModifiedType IDamageModifier.GetModifiedType()
	{
		return DamageModifiedType.Else;
	}

	DamagePipelinePosition IDamageModifier.GetPipelinePosition()
	{
		return DamagePipelinePosition.EndOfFinal;
	}

	long IBaseDamageModifier.GetOperationUid()
	{
		return base.OperationInfo.Uid;
	}

	bool IBaseDamageModifier.IsValid()
	{
		return true;
	}
}
