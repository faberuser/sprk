namespace NShared;

public class ModCooltimeInfoBase : SnapshotObject
{
	private readonly SnapshotObjectMember<int> _skillIndexHolder;

	private readonly SnapshotObjectMember<int> _countHolder;

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

	public int Count
	{
		get
		{
			return _countHolder.Get();
		}
		set
		{
			_countHolder.Set(value);
		}
	}

	public ModCooltimeInfoBase()
	{
		_skillIndexHolder = new SnapshotObjectMember<int>(this, 0);
		_countHolder = new SnapshotObjectMember<int>(this, 0);
	}
}
