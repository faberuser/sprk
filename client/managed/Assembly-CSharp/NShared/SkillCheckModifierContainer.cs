using System.Collections.Generic;
using System.Linq;

namespace NShared;

public class SkillCheckModifierContainer : SnapshotObject
{
	private readonly SnapshotObjectMember<BattleCreature> _ownerHolder;

	private readonly SnapshotObjectMember<SnapshotList<ISkillCheckSuppresser>> _suppresserListHolder;

	private readonly SnapshotObjectMember<SnapshotList<ISkillDisabler>> _disablerListHolder;

	public BattleCreature Owner => _ownerHolder.Get();

	public SnapshotList<ISkillCheckSuppresser> SuppresserList => _suppresserListHolder.Get();

	public SnapshotList<ISkillDisabler> DisablerList => _disablerListHolder.Get();

	public SkillCheckModifierContainer()
	{
		_ownerHolder = new SnapshotObjectMember<BattleCreature>(this);
		_suppresserListHolder = new SnapshotObjectMember<SnapshotList<ISkillCheckSuppresser>>(this);
		_disablerListHolder = new SnapshotObjectMember<SnapshotList<ISkillDisabler>>(this);
	}

	public void Init(BattleCreature creature)
	{
		_ownerHolder.Set(creature);
		_suppresserListHolder.Alloc();
		_disablerListHolder.Alloc();
	}

	public void Update(int deltaTimeMs)
	{
	}

	public void Add(ISkillCheckSuppresser enabler)
	{
		if (!SuppresserList.Contains(enabler))
		{
			SuppresserList.Add(enabler);
		}
	}

	public bool Remove(ISkillCheckSuppresser enabler)
	{
		return SuppresserList.Remove(enabler);
	}

	public void Add(ISkillDisabler enabler)
	{
		if (!DisablerList.Contains(enabler))
		{
			DisablerList.Add(enabler);
		}
	}

	public bool Remove(ISkillDisabler enabler)
	{
		return DisablerList.Remove(enabler);
	}

	public bool IsSuppressedCheck(int skillIndex, SkillCheckSuppressType suppressType)
	{
		for (int i = 0; i < SuppresserList.Count; i++)
		{
			ISkillCheckSuppresser skillCheckSuppresser = SuppresserList[i];
			if (skillCheckSuppresser == null || skillCheckSuppresser.GetSuppressType() != suppressType)
			{
				continue;
			}
			IEnumerable<int> skillIndexList = skillCheckSuppresser.GetSkillIndexList();
			if (skillIndexList == null)
			{
				continue;
			}
			foreach (int item in skillIndexList)
			{
				if (item == skillIndex)
				{
					return true;
				}
			}
		}
		return false;
	}

	public int[] GetSuppressedSkillIndices(SkillCheckSuppressType suppressType)
	{
		List<int> list = null;
		for (int i = 0; i < SuppresserList.Count; i++)
		{
			ISkillCheckSuppresser skillCheckSuppresser = SuppresserList[i];
			if (skillCheckSuppresser == null || skillCheckSuppresser.GetSuppressType() != suppressType)
			{
				continue;
			}
			IEnumerable<int> skillIndexList = skillCheckSuppresser.GetSkillIndexList();
			if (skillIndexList == null)
			{
				continue;
			}
			foreach (int item in skillIndexList)
			{
				list = list ?? new List<int>();
				list.Add(item);
			}
		}
		return list?.ToArray();
	}

	public bool IsDisabledSkill(int skillIndex, SkillDisablerType disablerType)
	{
		for (int i = 0; i < DisablerList.Count; i++)
		{
			ISkillDisabler skillDisabler = DisablerList[i];
			if (skillDisabler != null && skillDisabler.GetDisablerType() == disablerType)
			{
				IEnumerable<int> skillIndexList = skillDisabler.GetSkillIndexList();
				if (skillIndexList != null && skillIndexList.Contains(skillIndex))
				{
					return true;
				}
			}
		}
		return false;
	}

	public void CheckDisabledSkillSlot(ISkillChanger skillChanger)
	{
		for (int i = 0; i < DisablerList.Count; i++)
		{
			ISkillDisabler skillDisabler = DisablerList[i];
			if (skillDisabler == null || skillDisabler.GetDisablerType() != SkillDisablerType.DisableSkillSlot)
			{
				continue;
			}
			IEnumerable<int> skillIndexList = skillDisabler.GetSkillIndexList();
			if (skillIndexList != null)
			{
				int value = skillChanger.FromSkillIndex();
				if (skillIndexList.Contains(value))
				{
					int toSkillIndex = skillChanger.ToSkillIndex();
					skillDisabler.AddSkillIndex(toSkillIndex);
				}
			}
		}
	}

	public int[] GetDisabledSkillIndices(SkillDisablerType disablerType)
	{
		List<int> list = null;
		for (int i = 0; i < DisablerList.Count; i++)
		{
			ISkillDisabler skillDisabler = DisablerList[i];
			if (skillDisabler == null || skillDisabler.GetDisablerType() != disablerType)
			{
				continue;
			}
			IEnumerable<int> skillIndexList = skillDisabler.GetSkillIndexList();
			if (skillIndexList == null)
			{
				continue;
			}
			foreach (int item in skillIndexList)
			{
				list = list ?? new List<int>();
				list.Add(item);
			}
		}
		return list?.ToArray();
	}

	public bool IsDisabledAutoUseSkill(int skillIndex)
	{
		for (int i = 0; i < DisablerList.Count; i++)
		{
			ISkillDisabler skillDisabler = DisablerList[i];
			if (skillDisabler != null && skillDisabler.IsDisableAutoUse())
			{
				IEnumerable<int> skillIndexList = skillDisabler.GetSkillIndexList();
				if (skillIndexList != null && skillIndexList.Contains(skillIndex))
				{
					return true;
				}
			}
		}
		return false;
	}

	public int[] GetDisabledAutoUseSkillIndices()
	{
		List<int> list = null;
		for (int i = 0; i < DisablerList.Count; i++)
		{
			ISkillDisabler skillDisabler = DisablerList[i];
			if (skillDisabler == null || !skillDisabler.IsDisableAutoUse())
			{
				continue;
			}
			IEnumerable<int> skillIndexList = skillDisabler.GetSkillIndexList();
			if (skillIndexList == null)
			{
				continue;
			}
			foreach (int item in skillIndexList)
			{
				list = list ?? new List<int>();
				list.Add(item);
			}
		}
		return list?.ToArray();
	}
}
