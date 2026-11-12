using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class StateDataContainer : JitDataContainerDefault<string, StateData>, ISnapshotMemberSerializer
{
	public const string TABLENAME = "StateTable";

	public override TablePathDefine PathDefine => new TablePathDefine("StateTable");

	object ISnapshotMemberSerializer.From(string value, DumpedSnapshotContainer container)
	{
		return GetData(value);
	}

	string ISnapshotMemberSerializer.To(object value)
	{
		if (value is StateData stateData)
		{
			return stateData.GETKEY();
		}
		return null;
	}

	protected override void OnEndLoadFromJsonData()
	{
		base.OnEndLoadFromJsonData();
		foreach (KeyValuePair<string, StateData> item in base.BaseDic.JsonLoad_Get())
		{
			InitStateFlag(item.Value);
		}
	}

	private void InitStateFlag(StateData value)
	{
		value.StateFlag = StateFlag.None;
		if (value.CanMove)
		{
			value.StateFlag |= StateFlag.CanMove;
		}
		if (value.CanPhysicalSkill)
		{
			value.StateFlag |= StateFlag.CanPhysicalSkill;
		}
		if (value.CanMagicalSkill)
		{
			value.StateFlag |= StateFlag.CanMagicalSkill;
		}
		if (value.CanBaseAttack)
		{
			value.StateFlag |= StateFlag.CanBaseAttack;
		}
		if (value.CanBeTargetedByAlly)
		{
			value.StateFlag |= StateFlag.CanBeTargetedByAlly;
		}
		if (value.CanBeTargetedByEnemy)
		{
			value.StateFlag |= StateFlag.CanBeTargetedByEnemy;
		}
		if (value.CanBePhysicalCc)
		{
			value.StateFlag |= StateFlag.CanBePhysicalCc;
		}
		if (value.CanBeMagicalCc)
		{
			value.StateFlag |= StateFlag.CanBeMagicalCc;
		}
		if (value.CanBePhysicalDamaged)
		{
			value.StateFlag |= StateFlag.CanBePhysicalDamaged;
		}
		if (value.CanBeMagicalDamaged)
		{
			value.StateFlag |= StateFlag.CanBeMagicalDamaged;
		}
		if (value.CanHitPhysicalAttack)
		{
			value.StateFlag |= StateFlag.CanHitPhysicalAttack;
		}
		if (value.CanHitMagicalAttack)
		{
			value.StateFlag |= StateFlag.CanHitMagicalAttack;
		}
		if (value.CanPhysicalBlockAll)
		{
			value.StateFlag |= StateFlag.CanPhysicalBlockAll;
		}
		if (value.CanMagicalBlockAll)
		{
			value.StateFlag |= StateFlag.CanMagicalBlockAll;
		}
		if (value.CanPhysicalDodgeAll)
		{
			value.StateFlag |= StateFlag.CanPhysicalDodgeAll;
		}
		if (value.CanMagicalDodgeAll)
		{
			value.StateFlag |= StateFlag.CanMagicalDodgeAll;
		}
		if (value.IsSticky)
		{
			value.StateFlag |= StateFlag.IsSticky;
		}
		if (value.IsPositive)
		{
			value.StateFlag |= StateFlag.IsPositive;
		}
		if (value.IsNegative)
		{
			value.StateFlag |= StateFlag.IsNegative;
		}
		if (value.IsCc)
		{
			value.StateFlag |= StateFlag.IsCc;
		}
		if (value.CanPassiveSkill)
		{
			value.StateFlag |= StateFlag.CanPassiveSkill;
		}
		if (value.RejectReservedSkill)
		{
			value.StateFlag |= StateFlag.RejectReservedSkill;
		}
		if (value.CanUpdateCooltime)
		{
			value.StateFlag |= StateFlag.CanUpdateCooltime;
		}
	}
}
