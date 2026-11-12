namespace NShared;

public class OnCoolTime : BaseSkillTrigger
{
	private readonly SnapshotObjectMember<int> _durationTimeMsHolder;

	public override SkillTriggerType TriggerType => SkillTriggerType.OnCoolTime;

	public int DurationTimeMs
	{
		get
		{
			return _durationTimeMsHolder.Get();
		}
		set
		{
			_durationTimeMsHolder.Set(value);
		}
	}

	public OnCoolTime()
	{
		_durationTimeMsHolder = new SnapshotObjectMember<int>(this, 0);
	}
}
