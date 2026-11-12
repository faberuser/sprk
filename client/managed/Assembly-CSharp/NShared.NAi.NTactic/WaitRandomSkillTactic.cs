using System.Collections.Generic;

namespace NShared.NAi.NTactic;

public class WaitRandomSkillTactic : SnapshotObject, IAiTactic, ISnapshotObject
{
	private readonly SnapshotObjectMember<SkillData> _skillIndexHolder;

	private readonly SnapshotObjectMember<bool> _isSmartAiHolder;

	public SkillData SkillData => _skillIndexHolder.Get();

	public WaitRandomSkillTactic()
	{
		_skillIndexHolder = new SnapshotObjectMember<SkillData>(this, null, GameRoot.Instance.SkillDataContainer);
		_isSmartAiHolder = new SnapshotObjectMember<bool>(this, value: false);
	}

	public void InitSmartAi(bool isSmartAi)
	{
		_isSmartAiHolder.Set(isSmartAi);
	}

	public void SetFirstSkillIndex(int skillIndex)
	{
		SkillData data = GameRoot.Instance.SkillDataContainer.GetData(skillIndex);
		_skillIndexHolder.Set(data);
	}

	void IAiTactic.Update(AiContext context)
	{
		BattleCreature creature = context.Creature;
		if (creature == null)
		{
			return;
		}
		if (creature.SoulWeaponSkillController.CanUseSoulWeaponSkill())
		{
			SkillData basicSkillData = creature.SoulWeaponSkillController.BasicSkillData;
			if (creature.SkillController.UseSkill(basicSkillData, SkillUseOption.GetSoulWeaponSkillUseOption(), null, 0, isActiveSkill: false, 10) == SkillUseResult.Success)
			{
				creature.SoulWeaponSkillController.IncreaseUseSkillCount();
			}
			return;
		}
		bool flag = _isSmartAiHolder.Get();
		SkillData skillData = _skillIndexHolder.Get();
		if (skillData == null)
		{
			List<int> skillIndexList = creature.GetSkillIndexList(includeExtra: false);
			if (skillIndexList == null || skillIndexList.Count == 0)
			{
				return;
			}
			List<int> list = new List<int>();
			for (int num = skillIndexList.Count - 1; num >= 0; num--)
			{
				int num2 = skillIndexList[num];
				int num3 = num2;
				int resultSkillIndex = num2;
				bool isReplaced = false;
				if (creature.SkillController.SkillChangerContainer.GetChangedSkillIndex(num3, out resultSkillIndex, out isReplaced))
				{
					num2 = resultSkillIndex;
					num3 = (isReplaced ? resultSkillIndex : num3);
				}
				skillData = GameRoot.Instance.SkillDataContainer.GetData(num2);
				if (skillData == null || skillData.TriggerType != SkillTriggerType.None || creature.SkillController.SkillCheckModifierContainer.IsDisabledAutoUseSkill(num2))
				{
					continue;
				}
				if (flag)
				{
					bool num4 = creature.SkillController.CooltimeManager.IsInCooltime(num3);
					UsingSkillInfo usingSkillInfo = creature.SkillController.UsingSkillInfo;
					bool flag2 = usingSkillInfo != null && (usingSkillInfo.SkillData.Index == num2 || usingSkillInfo.FiredSkillData.Index == num2);
					if (num4 || flag2)
					{
						continue;
					}
				}
				if (creature.GetUserSkillLevel(num3) > 0)
				{
					list.Add(num3);
				}
			}
			if (list.Count == 0)
			{
				return;
			}
			int index = creature.NextRandom(list.Count, "IAiTactic.Update");
			int key = list[index];
			skillData = GameRoot.Instance.SkillDataContainer.GetData(key);
			_skillIndexHolder.Set(skillData);
		}
		if (flag && skillData != null && creature.SkillController.CooltimeManager.IsInCooltime(skillData.Index))
		{
			_skillIndexHolder.Set(null);
		}
		else if (skillData != null)
		{
			if (creature.SkillController.CanUseSkill(skillData, new SkillUseOption(checkOnly: true)) != SkillUseResult.Success)
			{
				_skillIndexHolder.Set(null);
				return;
			}
			_skillIndexHolder.Set(null);
			creature.SkillController.UseSkill(skillData, new SkillUseOption(), null, 0, isActiveSkill: true);
		}
	}
}
