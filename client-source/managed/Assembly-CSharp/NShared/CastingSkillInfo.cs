namespace NShared;

public class CastingSkillInfo : SnapshotObject
{
	private readonly SnapshotObjectMember<SkillData> _skillDataHolder;

	private readonly SnapshotObjectMember<int> _skillLevelHolder;

	private readonly SnapshotObjectMember<int> _elapsedTimeMsHolder;

	private readonly SnapshotObjectMember<ISkillTriggerInfo> _triggerInfoHolder;

	private readonly SnapshotObjectMember<SnapshotArray<BattleCreature>> _targetCreaturesHolder;

	private readonly SnapshotObjectMember<BattleCreature> _manualTargetHolder;

	public SkillData SkillData
	{
		get
		{
			return _skillDataHolder.Get();
		}
		set
		{
			_skillDataHolder.Set(value);
		}
	}

	public int SkillLevel
	{
		get
		{
			return _skillLevelHolder.Get();
		}
		set
		{
			_skillLevelHolder.Set(value);
		}
	}

	public int ElapsedTimeMs
	{
		get
		{
			return _elapsedTimeMsHolder.Get();
		}
		set
		{
			_elapsedTimeMsHolder.Set(value);
		}
	}

	public ISkillTriggerInfo TriggerInfo
	{
		get
		{
			return _triggerInfoHolder.Get();
		}
		set
		{
			_triggerInfoHolder.Set(value);
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

	public BattleCreature ManualTarget
	{
		get
		{
			return _manualTargetHolder.Get();
		}
		set
		{
			_manualTargetHolder.Set(value);
		}
	}

	public CastingSkillInfo()
	{
		_skillDataHolder = new SnapshotObjectMember<SkillData>(this, null, GameRoot.Instance.SkillDataContainer);
		_skillLevelHolder = new SnapshotObjectMember<int>(this, 0);
		_elapsedTimeMsHolder = new SnapshotObjectMember<int>(this, 0);
		_triggerInfoHolder = new SnapshotObjectMember<ISkillTriggerInfo>(this);
		_targetCreaturesHolder = new SnapshotObjectMember<SnapshotArray<BattleCreature>>(this);
		_manualTargetHolder = new SnapshotObjectMember<BattleCreature>(this);
	}

	public void Update(int deltaTimeMs)
	{
		ElapsedTimeMs += deltaTimeMs;
	}

	public bool IsCasting()
	{
		if (SkillData == null)
		{
			return false;
		}
		return ElapsedTimeMs < SkillData.CastingTimeMs;
	}

	public bool IsSoloTime()
	{
		if (SkillData == null)
		{
			return false;
		}
		return ElapsedTimeMs < SkillData.SoloTimeMs;
	}
}
