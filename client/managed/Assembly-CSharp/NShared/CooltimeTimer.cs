namespace NShared;

public class CooltimeTimer : SnapshotObject
{
	private readonly SnapshotObjectMember<int> _skillIndexHolder;

	private readonly SnapshotObjectMember<int> _elapsedTimeMsHolder;

	private readonly SnapshotObjectMember<int> _initDurationMsHolder;

	private readonly SnapshotObjectMember<int> _addDurationRatioHolder;

	private readonly SnapshotObjectMember<int> _addDurationMsHolder;

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

	public int InitDurationMs
	{
		get
		{
			return _initDurationMsHolder.Get();
		}
		set
		{
			_initDurationMsHolder.Set(value);
		}
	}

	public int AddDurationMs
	{
		get
		{
			return _addDurationMsHolder.Get();
		}
		set
		{
			_addDurationMsHolder.Set(value);
		}
	}

	public int AddDurationRatio
	{
		get
		{
			return _addDurationRatioHolder.Get();
		}
		set
		{
			_addDurationRatioHolder.Set(value);
		}
	}

	public CooltimeTimer()
	{
		_skillIndexHolder = new SnapshotObjectMember<int>(this, 0);
		_elapsedTimeMsHolder = new SnapshotObjectMember<int>(this, 0);
		_initDurationMsHolder = new SnapshotObjectMember<int>(this, 0);
		_addDurationRatioHolder = new SnapshotObjectMember<int>(this, 0);
		_addDurationMsHolder = new SnapshotObjectMember<int>(this, 0);
	}

	public CooltimeTimer(int skillIndex, int durationTimeMs)
		: this()
	{
		_skillIndexHolder.Set(skillIndex);
		_initDurationMsHolder.Set(durationTimeMs);
	}
}
