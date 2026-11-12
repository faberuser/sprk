namespace NShared;

public class PassiveSkillInfo : SnapshotObject
{
	private readonly SnapshotObjectMember<SkillData> _skillDataHolder;

	private readonly SnapshotObjectMember<int> _skillLevelHolder;

	private readonly SnapshotObjectMember<long> _triggerTimeMsHolder;

	private readonly SnapshotObjectMember<long> _triggerStartTimeMsHolder;

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

	public long TriggerTimeMs
	{
		get
		{
			return _triggerTimeMsHolder.Get();
		}
		set
		{
			_triggerTimeMsHolder.Set(value);
		}
	}

	public long TriggerStartTimeMs
	{
		get
		{
			return _triggerStartTimeMsHolder.Get();
		}
		set
		{
			_triggerStartTimeMsHolder.Set(value);
		}
	}

	public int SkillIndex => SkillData.Index;

	public SkillTriggerType TriggerType => SkillData.TriggerType;

	public PassiveSkillInfo()
	{
		_skillDataHolder = new SnapshotObjectMember<SkillData>(this, null, GameRoot.Instance.SkillDataContainer);
		_skillLevelHolder = new SnapshotObjectMember<int>(this, 0);
		_triggerTimeMsHolder = new SnapshotObjectMember<long>(this, 0L);
		_triggerStartTimeMsHolder = new SnapshotObjectMember<long>(this, 0L);
	}

	public void Init(SkillData skillData, int skillLevel, long triggerTimeMs)
	{
		_skillDataHolder.Set(skillData);
		_skillLevelHolder.Set(skillLevel);
		if (skillData.TriggerType == SkillTriggerType.OnTimer)
		{
			_triggerTimeMsHolder.Set(0L);
			_triggerStartTimeMsHolder.Set(triggerTimeMs);
		}
	}

	public void UpdateTriggerTime(long deltaTimeMs)
	{
		TriggerTimeMs += deltaTimeMs;
	}

	public bool IsTriggerTimePassed()
	{
		return TriggerStartTimeMs <= TriggerTimeMs;
	}

	public void InitTriggerTime(long triggerTimeMs)
	{
		TriggerTimeMs = 0L;
		TriggerStartTimeMs = triggerTimeMs;
	}
}
