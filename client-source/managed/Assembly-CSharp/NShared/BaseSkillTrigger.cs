namespace NShared;

public abstract class BaseSkillTrigger : SnapshotObject, ISkillTriggerInfo, ISnapshotObject
{
	private readonly SnapshotObjectMember<BattleCreature> _triggerCreatureHolder;

	private readonly SnapshotObjectMember<SnapshotArray<BattleCreature>> _targetCreaturesHolder;

	private readonly SnapshotObjectMember<int> _skillIndexHolder;

	public abstract SkillTriggerType TriggerType { get; }

	public BattleCreature TriggerCreature
	{
		get
		{
			return _triggerCreatureHolder.Get();
		}
		set
		{
			_triggerCreatureHolder.Set(value);
		}
	}

	public BattleCreature[] TargetCreatures
	{
		get
		{
			return _targetCreaturesHolder.GET();
		}
		set
		{
			_targetCreaturesHolder.Set(value);
		}
	}

	public int SkillIndex
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

	public BaseSkillTrigger()
	{
		_triggerCreatureHolder = new SnapshotObjectMember<BattleCreature>(this);
		_targetCreaturesHolder = new SnapshotObjectMember<SnapshotArray<BattleCreature>>(this);
		_skillIndexHolder = new SnapshotObjectMember<int>(this, 0);
	}

	public virtual bool CheckTriggerValue(BattleCreature firedCreature, ITriggerValueGetter valueGetter)
	{
		int[] values = null;
		if (valueGetter.GetTriggerValues(firedCreature, 0, out values) && values != null && values.Length != 0 && values[0] != 0 && !TypeIterator.Contains(values, SkillIndex))
		{
			return false;
		}
		return true;
	}
}
