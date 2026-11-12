namespace NShared;

public class SkillTargetController : SnapshotObject
{
	private readonly SnapshotObjectMember<BattleCreature> _ownerHolder;

	private readonly SnapshotObjectMember<SnapshotList<ISkillTargetFilter>> _skillTargetFilterListHolder;

	private readonly SnapshotObjectMember<bool> _ignoreFilterOnCheckOnly;

	public SkillTargetController()
	{
		_ownerHolder = new SnapshotObjectMember<BattleCreature>(this);
		_skillTargetFilterListHolder = new SnapshotObjectMember<SnapshotList<ISkillTargetFilter>>(this);
		_ignoreFilterOnCheckOnly = new SnapshotObjectMember<bool>(this, value: false);
	}

	public void Init(BattleCreature owner)
	{
		_ownerHolder.Set(owner);
		_skillTargetFilterListHolder.Alloc();
	}

	public void Update(int deltaTimeMs)
	{
	}

	public void SetIgnoreFilterOnCheckOnly(bool ignoreFilterOnCheckOnly)
	{
		_ignoreFilterOnCheckOnly.Set(ignoreFilterOnCheckOnly);
	}

	public bool GetIgnoreFilterOnCheckOnly()
	{
		return _ignoreFilterOnCheckOnly.Get();
	}

	public void AddTargetFilter(ISkillTargetFilter filter)
	{
		SnapshotList<ISkillTargetFilter> snapshotList = _skillTargetFilterListHolder.Get();
		if (!snapshotList.Contains(filter))
		{
			snapshotList.Add(filter);
		}
	}

	public bool RemoveTargetFilter(ISkillTargetFilter filter)
	{
		return _skillTargetFilterListHolder.Get().Remove(filter);
	}

	public bool HasSkillTargetFilter()
	{
		return _skillTargetFilterListHolder.Get().Count > 0;
	}

	public void FilterTarget(ref BattleCreatureList creatureList, SkillTargetFilterInfo filterInfo)
	{
		SnapshotList<ISkillTargetFilter> snapshotList = _skillTargetFilterListHolder.Get();
		for (int i = 0; i < snapshotList.Count; i++)
		{
			ISkillTargetFilter skillTargetFilter = snapshotList[i];
			if (skillTargetFilter != null)
			{
				if (creatureList == null)
				{
					break;
				}
				skillTargetFilter.Filter(ref creatureList, filterInfo);
			}
		}
	}
}
