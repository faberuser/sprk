using System.Collections.Generic;

namespace NShared;

public class SkillChangerContainer : SnapshotObject
{
	private readonly SnapshotObjectMember<BattleCreature> _ownerHolder;

	private readonly SnapshotObjectMember<SnapshotList<ISkillChanger>> _changerListHolder;

	public BattleCreature Owner => _ownerHolder.Get();

	public SnapshotList<ISkillChanger> ChangerList => _changerListHolder.Get();

	public SkillChangerContainer()
	{
		_ownerHolder = new SnapshotObjectMember<BattleCreature>(this);
		_changerListHolder = new SnapshotObjectMember<SnapshotList<ISkillChanger>>(this);
	}

	public void Init(BattleCreature creature)
	{
		_ownerHolder.Set(creature);
		_changerListHolder.Alloc();
	}

	public void Update(int deltaTimeMs)
	{
	}

	public void Register(ISkillChanger changer)
	{
		if (!ChangerList.Contains(changer))
		{
			ChangerList.Add(changer);
		}
	}

	public bool Unregister(ISkillChanger changer)
	{
		return ChangerList.Remove(changer);
	}

	public bool GetChangedSkillIndex(int skillIndex, out int resultSkillIndex)
	{
		for (int i = 0; i < ChangerList.Count; i++)
		{
			ISkillChanger skillChanger = ChangerList[i];
			if (skillChanger != null && skillChanger.FromSkillIndex() == skillIndex)
			{
				resultSkillIndex = skillChanger.ToSkillIndex();
				return true;
			}
		}
		resultSkillIndex = skillIndex;
		return false;
	}

	public bool GetChangedSkillIndex(int skillIndex, out int resultSkillIndex, out bool isReplaced)
	{
		for (int i = 0; i < ChangerList.Count; i++)
		{
			ISkillChanger skillChanger = ChangerList[i];
			if (skillChanger != null && skillChanger.FromSkillIndex() == skillIndex)
			{
				resultSkillIndex = skillChanger.ToSkillIndex();
				isReplaced = skillChanger.IsReplaced();
				return true;
			}
		}
		resultSkillIndex = skillIndex;
		isReplaced = false;
		return false;
	}

	public int[] GetChangedSkillIndices()
	{
		List<int> list = null;
		for (int i = 0; i < ChangerList.Count; i++)
		{
			ISkillChanger skillChanger = ChangerList[i];
			if (skillChanger != null && skillChanger.IsReplaced())
			{
				list = list ?? new List<int>();
				list.Add(skillChanger.FromSkillIndex());
				list.Add(skillChanger.ToSkillIndex());
			}
		}
		return list?.ToArray();
	}
}
