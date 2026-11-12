namespace NShared;

public class ModCooltimeDurationInfo : ModCooltimeInfoBase
{
	private readonly SnapshotObjectMember<int> _durationMsHolder;

	public int DurationMs
	{
		get
		{
			return _durationMsHolder.Get();
		}
		set
		{
			_durationMsHolder.Set(value);
		}
	}

	public ModCooltimeDurationInfo()
	{
		_durationMsHolder = new SnapshotObjectMember<int>(this, 0);
	}
}
