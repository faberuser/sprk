using System.Collections.Generic;

namespace NShared;

public class DungeonSkillController : SnapshotObject
{
	private readonly SnapshotObjectMember<SnapshotList<DungeonSkill>> _dungeonSkillListHolder;

	private bool _hasDungeonSkill;

	public SnapshotList<DungeonSkill> DungeonSkillList => _dungeonSkillListHolder.Get();

	public DungeonSkillController()
	{
		_dungeonSkillListHolder = new SnapshotObjectMember<SnapshotList<DungeonSkill>>(this);
	}

	public void Init(CampaignDungeonData dungeonData, BattleCreature owner)
	{
		if (owner == null || owner.CreateInfo == null)
		{
			_hasDungeonSkill = false;
			return;
		}
		_hasDungeonSkill = owner.CreateInfo.HasDungeonSkill;
		if (!_hasDungeonSkill)
		{
			return;
		}
		if (dungeonData == null || dungeonData.DungeonSkillIndexList == null || dungeonData.DungeonSkillIndexList.Length == 0)
		{
			_hasDungeonSkill = false;
			return;
		}
		_dungeonSkillListHolder.Alloc();
		for (int i = 0; i < dungeonData.DungeonSkillIndexList.Length; i++)
		{
			int num = dungeonData.DungeonSkillIndexList[i];
			if (num != 0)
			{
				int slotIndex = i + 1;
				DungeonSkill dungeonSkill = SnapshotObjectPool<DungeonSkill>.Alloc();
				if (!dungeonSkill.Init(owner, num, slotIndex))
				{
					SnapshotObjectPool<DungeonSkill>.ReleaseImmediately(dungeonSkill);
				}
				else
				{
					_dungeonSkillListHolder.Get().Add(dungeonSkill);
				}
			}
		}
	}

	public void Update()
	{
		if (_hasDungeonSkill)
		{
			for (int i = 0; i < DungeonSkillList.Count; i++)
			{
				DungeonSkillList[i]?.Update();
			}
		}
	}

	public SkillUseResult CanUseSkill(int skillIndex)
	{
		if (!_hasDungeonSkill)
		{
			return SkillUseResult.Fail;
		}
		if (DungeonSkillList == null || DungeonSkillList.Count == 0)
		{
			return SkillUseResult.Fail;
		}
		SkillUseResult result = SkillUseResult.Fail;
		for (int i = 0; i < DungeonSkillList.Count; i++)
		{
			DungeonSkill dungeonSkill = DungeonSkillList[i];
			if (dungeonSkill != null && (dungeonSkill.BaseSkillIndex == skillIndex || dungeonSkill.EnhancedSkillIndex == skillIndex))
			{
				result = dungeonSkill.SkillUseResult;
				break;
			}
		}
		return result;
	}

	public DungeonSkillStateInfo[] GetDungeonSkillStateInfos()
	{
		List<DungeonSkillStateInfo> list = null;
		for (int i = 0; i < DungeonSkillList.Count; i++)
		{
			DungeonSkill dungeonSkill = DungeonSkillList[i];
			if (dungeonSkill != null)
			{
				list = list ?? new List<DungeonSkillStateInfo>();
				DungeonSkillStateInfo item = new DungeonSkillStateInfo
				{
					SkillSlotIndex = dungeonSkill.SkillSlotIndex,
					BaseSkillIndex = dungeonSkill.BaseSkillIndex,
					BaseStateRatioM = dungeonSkill.GetBaseStateRatioM(),
					EnhancedSkillIndex = dungeonSkill.EnhancedSkillIndex,
					EnhancedStateRatioM = dungeonSkill.GetEnhancedStateRatioM(),
					UseableSkillCount = dungeonSkill.UseableSkillCount,
					SkillCanUseResult = dungeonSkill.SkillUseResult
				};
				list.Add(item);
			}
		}
		return list?.ToArray();
	}

	public bool IsValidUseCount(int slotIndex)
	{
		bool result = false;
		DungeonSkill dungeonSkillBySlotIndex = GetDungeonSkillBySlotIndex(slotIndex);
		if (dungeonSkillBySlotIndex != null)
		{
			result = dungeonSkillBySlotIndex.IsValidUseCount();
		}
		return result;
	}

	public void IncreaseUseSkillCount(int slotIndex, int skillIndex)
	{
		DungeonSkill dungeonSkillBySlotIndex = GetDungeonSkillBySlotIndex(slotIndex);
		if (dungeonSkillBySlotIndex != null && (dungeonSkillBySlotIndex.BaseSkillIndex == skillIndex || dungeonSkillBySlotIndex.EnhancedSkillIndex == skillIndex))
		{
			dungeonSkillBySlotIndex.IncreaseUseSkillCount();
		}
	}

	public DungeonSkillStateInfo GetDungeonSkillInfo(int slotIndex)
	{
		DungeonSkill dungeonSkillBySlotIndex = GetDungeonSkillBySlotIndex(slotIndex);
		if (dungeonSkillBySlotIndex == null)
		{
			return null;
		}
		return new DungeonSkillStateInfo
		{
			SkillSlotIndex = dungeonSkillBySlotIndex.SkillSlotIndex,
			BaseSkillIndex = dungeonSkillBySlotIndex.BaseSkillIndex,
			BaseStateRatioM = dungeonSkillBySlotIndex.GetBaseStateRatioM(),
			EnhancedSkillIndex = dungeonSkillBySlotIndex.EnhancedSkillIndex,
			EnhancedStateRatioM = dungeonSkillBySlotIndex.GetEnhancedStateRatioM(),
			UseableSkillCount = dungeonSkillBySlotIndex.UseableSkillCount,
			SkillCanUseResult = dungeonSkillBySlotIndex.SkillUseResult
		};
	}

	private DungeonSkill GetDungeonSkillBySlotIndex(int slotIndex)
	{
		if (!_hasDungeonSkill || DungeonSkillList == null || DungeonSkillList.Count == 0)
		{
			return null;
		}
		DungeonSkill result = null;
		for (int i = 0; i < DungeonSkillList.Count; i++)
		{
			DungeonSkill dungeonSkill = DungeonSkillList[i];
			if (dungeonSkill != null && dungeonSkill.SkillSlotIndex == slotIndex)
			{
				result = dungeonSkill;
				break;
			}
		}
		return result;
	}

	public bool HasDungeonSkill()
	{
		return _hasDungeonSkill;
	}
}
