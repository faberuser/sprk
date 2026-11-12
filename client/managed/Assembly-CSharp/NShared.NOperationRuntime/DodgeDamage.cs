using System;

namespace NShared.NOperationRuntime;

public class DodgeDamage : BaseOperationRuntime, IDamageDodger, IBaseDamageModifier, ISnapshotObject
{
	protected readonly SnapshotObjectMember<int> _modifyCountHolder;

	public DodgeDamage()
	{
		_modifyCountHolder = new SnapshotObjectMember<int>(this, 0);
	}

	protected override void OnInit()
	{
		int valueInt = base.OperationInfo.GetValueInt(0);
		_modifyCountHolder.Set(valueInt);
	}

	protected override void OnBegin()
	{
		base.OnBegin();
		base.OperationInfo.Creature.StatController.AddModifier(this);
	}

	protected override void OnEnd()
	{
		base.OnEnd();
		base.OperationInfo.Creature.StatController.RemoveModifier(this);
	}

	bool IDamageDodger.Dodge()
	{
		if (_modifyCountHolder.Get() <= 0)
		{
			return false;
		}
		int data = Math.Max(_modifyCountHolder.Get() - 1, 0);
		_modifyCountHolder.Set(data);
		return true;
	}

	long IBaseDamageModifier.GetOperationUid()
	{
		return base.OperationInfo.Uid;
	}

	bool IBaseDamageModifier.IsValid()
	{
		return _modifyCountHolder.Get() > 0;
	}
}
