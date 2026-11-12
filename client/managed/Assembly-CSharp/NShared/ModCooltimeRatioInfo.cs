namespace NShared;

public class ModCooltimeRatioInfo : ModCooltimeInfoBase
{
	private readonly SnapshotObjectMember<int> _ratioMHolder;

	public int RatioM
	{
		get
		{
			return _ratioMHolder.Get();
		}
		set
		{
			_ratioMHolder.Set(value);
		}
	}

	public ModCooltimeRatioInfo()
	{
		_ratioMHolder = new SnapshotObjectMember<int>(this, 0);
	}
}
