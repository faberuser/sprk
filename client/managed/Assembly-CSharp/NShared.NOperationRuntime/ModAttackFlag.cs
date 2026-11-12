namespace NShared.NOperationRuntime;

public class ModAttackFlag : BaseOperationRuntime, IDamageFlagModifier, IBaseDamageModifier, ISnapshotObject
{
	protected readonly SnapshotObjectMember<DamageFlag> _addFlagHolder;

	protected readonly SnapshotObjectMember<DamageFlag> _removeFlagHolder;

	public ModAttackFlag()
	{
		_addFlagHolder = new SnapshotObjectMember<DamageFlag>(this, DamageFlag.None);
		_removeFlagHolder = new SnapshotObjectMember<DamageFlag>(this, DamageFlag.None);
	}

	protected override void OnInit()
	{
		_addFlagHolder.Set(base.OperationInfo.GetValueFlag<DamageFlag>(0));
		_removeFlagHolder.Set(base.OperationInfo.GetValueFlag<DamageFlag>(1));
	}

	protected override void OnBegin()
	{
		base.OnBegin();
		base.OperationInfo.Creature.StatController.AddFlagModifier(this);
	}

	protected override void OnEnd()
	{
		base.OnEnd();
		base.OperationInfo.Creature.StatController.RemoveFlagModifier(this);
	}

	bool IDamageFlagModifier.Modify(BattleCreature damaged, ref DamageFlag damageFlag)
	{
		if (_addFlagHolder.Get() != DamageFlag.None)
		{
			damageFlag |= _addFlagHolder.Get();
		}
		if (_removeFlagHolder.Get() != DamageFlag.None)
		{
			damageFlag &= ~_removeFlagHolder.Get();
		}
		return true;
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
