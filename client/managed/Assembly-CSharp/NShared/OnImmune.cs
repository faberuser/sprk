namespace NShared;

public class OnImmune : BaseSkillTrigger
{
	private readonly SnapshotObjectMember<DamageFlag> _damageFlagHolder;

	public override SkillTriggerType TriggerType => SkillTriggerType.OnImmune;

	public DamageFlag DamageFlag
	{
		get
		{
			return _damageFlagHolder.Get();
		}
		set
		{
			_damageFlagHolder.Set(value);
		}
	}

	public OnImmune()
	{
		_damageFlagHolder = new SnapshotObjectMember<DamageFlag>(this, DamageFlag.None);
	}

	public override bool CheckTriggerValue(BattleCreature firedCreature, ITriggerValueGetter valueGetter)
	{
		if (!base.CheckTriggerValue(firedCreature, valueGetter))
		{
			return false;
		}
		if (valueGetter.GetTriggerValueT<DamageFlag>(firedCreature, 1, out var value) && value != DamageFlag.None && (value & DamageFlag) == 0)
		{
			return false;
		}
		return true;
	}
}
