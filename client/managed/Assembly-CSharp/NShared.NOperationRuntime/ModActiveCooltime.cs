using System;

namespace NShared.NOperationRuntime;

public class ModActiveCooltime : BaseOperationRuntime
{
	[Flags]
	public enum ActiveSkillFlag
	{
		None = 0,
		InCoolTime = 1,
		NotInCoolTime = 2,
		UsingSkill = 4
	}

	public const int MaxActiveSkillSlot = 3;

	public static int[] DefaultSlotIndices;

	static ModActiveCooltime()
	{
		DefaultSlotIndices = new int[3];
		for (int i = 1; i <= 3; i++)
		{
			DefaultSlotIndices[i - 1] = i;
		}
	}

	protected override void OnBegin()
	{
		base.OnBegin();
		int valueInt = base.OperationInfo.GetValueInt(0);
		int valueInt2 = base.OperationInfo.GetValueInt(1);
		ActiveSkillFlag valueFlag = base.OperationInfo.GetValueFlag<ActiveSkillFlag>(2);
		int valueInt3 = base.OperationInfo.GetValueInt(3);
		BattleCreature creature = base.OperationInfo.Creature;
		if (creature != null)
		{
			if (valueInt3 == 0)
			{
				SetCoolTimeByActiveSkillCount(creature, valueInt, valueFlag, valueInt2);
			}
			else
			{
				SetCoolTimeByTargetSlotNo(creature, valueInt3, valueFlag, valueInt2);
			}
		}
	}

	private void SetCoolTimeByActiveSkillCount(BattleCreature creature, int activeSkillCount, ActiveSkillFlag flag, int durationMs)
	{
		ForEachActiveSkillIndex(creature, activeSkillCount, flag, delegate(int skillIndex)
		{
			if (GameRoot.Instance.SkillDataContainer.GetData(skillIndex) != null)
			{
				SetCoolTime(creature, skillIndex, durationMs);
			}
		});
	}

	private void SetCoolTimeByTargetSlotNo(BattleCreature creature, int slotNo, ActiveSkillFlag flag, int durationMs)
	{
		int skillIndexBySlot = creature.CreatureData.GetSkillIndexBySlot(slotNo);
		if (CheckActiveSkillFlag(creature, flag, skillIndexBySlot))
		{
			SetCoolTime(creature, skillIndexBySlot, durationMs);
		}
		int[] linkedSkillIndexBySlot = creature.CreatureData.GetLinkedSkillIndexBySlot(slotNo);
		if (linkedSkillIndexBySlot == null || linkedSkillIndexBySlot.Length == 0)
		{
			return;
		}
		foreach (int skillIndex in linkedSkillIndexBySlot)
		{
			if (CheckActiveSkillFlag(creature, flag, skillIndex))
			{
				SetCoolTime(creature, skillIndex, durationMs);
			}
		}
	}

	public static void ForEachActiveSkillIndex(BattleCreature creature, int activeSkillCount, ActiveSkillFlag flag, Action<int> callback)
	{
		if (creature == null || callback == null)
		{
			return;
		}
		if (activeSkillCount == 0 || activeSkillCount >= 3)
		{
			for (int i = 0; i < DefaultSlotIndices.Length; i++)
			{
				int slotNo = DefaultSlotIndices[i];
				int skillIndexBySlot = creature.CreatureData.GetSkillIndexBySlot(slotNo);
				if (CheckActiveSkillFlag(creature, flag, skillIndexBySlot))
				{
					callback(skillIndexBySlot);
				}
			}
			int[] extraActiveSkill = creature.CreatureData.ExtraActiveSkill;
			if (extraActiveSkill == null)
			{
				return;
			}
			foreach (int num in extraActiveSkill)
			{
				if (CheckActiveSkillFlag(creature, flag, num))
				{
					callback(num);
				}
			}
			return;
		}
		int[] array = ArrayPool<int>.Alloc(3);
		for (int k = 1; k <= 3; k++)
		{
			array[k - 1] = k;
		}
		creature.Shuffle(array);
		int num2 = 0;
		foreach (int slotNo2 in array)
		{
			int skillIndexBySlot2 = creature.CreatureData.GetSkillIndexBySlot(slotNo2);
			if (CheckActiveSkillFlag(creature, flag, skillIndexBySlot2))
			{
				callback(skillIndexBySlot2);
				if (++num2 >= activeSkillCount)
				{
					break;
				}
			}
		}
		ArrayPool<int>.Release(array);
	}

	public static bool CheckActiveSkillFlag(BattleCreature creature, ActiveSkillFlag flag, int skillIndex)
	{
		if ((flag & ActiveSkillFlag.InCoolTime) != ActiveSkillFlag.None && !creature.SkillController.CooltimeManager.IsInCooltime(skillIndex))
		{
			return false;
		}
		if ((flag & ActiveSkillFlag.NotInCoolTime) != ActiveSkillFlag.None && creature.SkillController.CooltimeManager.IsInCooltime(skillIndex))
		{
			return false;
		}
		if ((flag & ActiveSkillFlag.UsingSkill) != ActiveSkillFlag.None && (creature.SkillController.UsingSkillInfo == null || creature.SkillController.UsingSkillInfo.SkillIndex != skillIndex) && !creature.SkillController.CooltimeManager.IsInCooltime(skillIndex))
		{
			return false;
		}
		SkillData data = GameRoot.Instance.SkillDataContainer.GetData(skillIndex);
		if (data != null && data.HasTriggerFlag(SkillTriggerFlag.NoActiveCoolTime))
		{
			return false;
		}
		return true;
	}

	public static void SetCoolTimeByRatio(BattleCreature creature, int skillIndex, int ratioM)
	{
		SetCoolTimeR(creature, skillIndex, ratioM);
	}

	public static void SetCoolTimeR(BattleCreature creature, int skillIndex, int ratioM)
	{
		SkillData data = GameRoot.Instance.SkillDataContainer.GetData(skillIndex);
		if (data == null)
		{
			return;
		}
		if (creature.SkillController.CooltimeManager.IsInCooltime(skillIndex))
		{
			creature.SkillController.CooltimeManager.AddCooltimeR(skillIndex, ratioM);
			return;
		}
		int num = data.GetCoolTimeMs(creature) * ratioM / 1000;
		if (creature.SkillController.UsingSkillInfo != null && creature.SkillController.UsingSkillInfo.SkillIndex == skillIndex)
		{
			creature.SkillController.CooltimeManager.ModNextCooltimeDuration(skillIndex, num, 0, checkExist: false);
		}
		else
		{
			creature.SkillController.CooltimeManager.BeginCooltime(skillIndex, num, applyCoolTimeModifier: false);
		}
	}

	public static void SetCoolTime(BattleCreature creature, int skillIndex, int durationMs)
	{
		if (creature.SkillController.CooltimeManager.IsInCooltime(skillIndex))
		{
			creature.SkillController.CooltimeManager.AddCooltime(skillIndex, durationMs);
		}
		else if (creature.SkillController.UsingSkillInfo != null && creature.SkillController.UsingSkillInfo.SkillIndex == skillIndex)
		{
			creature.SkillController.CooltimeManager.ModNextCooltimeDuration(skillIndex, durationMs, 0, checkExist: false);
		}
		else
		{
			creature.SkillController.CooltimeManager.BeginCooltime(skillIndex, durationMs, applyCoolTimeModifier: false);
		}
	}
}
