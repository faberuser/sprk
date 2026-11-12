using System;
using System.Collections.Generic;

namespace NShared;

public class StatModifierBlock : IDisposable
{
	private readonly BattleCreature _creature;

	private readonly List<ITriggerStatModifier> _modifiers;

	public StatModifierBlock(BattleCreature creature, SkillTriggerType triggerType, int skillIndex)
	{
		if (creature == null)
		{
			return;
		}
		_creature = creature;
		_modifiers = GetTriggerStatModifierList(creature, triggerType, skillIndex);
		if (_modifiers == null)
		{
			return;
		}
		for (int i = 0; i < _modifiers.Count; i++)
		{
			ITriggerStatModifier triggerStatModifier = _modifiers[i];
			if (triggerStatModifier != null)
			{
				triggerStatModifier.DecCount();
				_creature.StatController.AddModifier(triggerStatModifier);
			}
		}
	}

	void IDisposable.Dispose()
	{
		if (_modifiers != null)
		{
			for (int i = 0; i < _modifiers.Count; i++)
			{
				_creature.StatController.RemoveModifier(_modifiers[i]);
			}
		}
	}

	private static List<ITriggerStatModifier> GetTriggerStatModifierList(BattleCreature creature, SkillTriggerType triggerType, int skillIndex)
	{
		if (creature == null)
		{
			return null;
		}
		SnapshotLinkedList<ITriggerStatModifier> triggerStatModifierList = creature.StatController.TriggerStatModifierList;
		if (triggerStatModifierList == null || triggerStatModifierList.Count == 0)
		{
			return null;
		}
		List<ITriggerStatModifier> list = null;
		LinkedListNode<ITriggerStatModifier> linkedListNode = triggerStatModifierList.First;
		while (linkedListNode != null)
		{
			ITriggerStatModifier value = linkedListNode.Value;
			linkedListNode = linkedListNode.Next;
			if (value.GetTriggerType() != triggerType)
			{
				continue;
			}
			int[] skillIndex2 = value.GetSkillIndex();
			if (skillIndex2 == null || skillIndex2.Length == 0 || TypeIterator.Contains(skillIndex2, skillIndex))
			{
				if (list == null)
				{
					list = new List<ITriggerStatModifier>();
				}
				list.Add(value);
			}
		}
		return list;
	}
}
