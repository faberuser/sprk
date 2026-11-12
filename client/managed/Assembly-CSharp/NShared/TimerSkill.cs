using System.Collections.Generic;

namespace NShared;

public class TimerSkill : BaseBattleTactic
{
	private readonly SnapshotObjectMember<SnapshotList<int>> _timeMsHolder;

	private readonly SnapshotObjectMember<SnapshotList<int>> _skillIndexHolder;

	private readonly SnapshotObjectMember<int> _stepHolder;

	public int Step => _stepHolder.Get();

	public int RemainNextStepTimeMs => GetRemainNextStepTimeMs();

	public TimerSkill()
	{
		_timeMsHolder = new SnapshotObjectMember<SnapshotList<int>>(this);
		_skillIndexHolder = new SnapshotObjectMember<SnapshotList<int>>(this);
		_stepHolder = new SnapshotObjectMember<int>(this, 0);
	}

	public override void OnInit()
	{
		if (base.TacticData.TacticValue != null)
		{
			_timeMsHolder.Alloc();
			_skillIndexHolder.Alloc();
			for (int i = 0; i < base.TacticData.TacticValue.Length / 2; i++)
			{
				int valueInt = ArgParser.GetValueInt(base.TacticData.TacticValue, i * 2);
				int valueInt2 = ArgParser.GetValueInt(base.TacticData.TacticValue, i * 2 + 1);
				_timeMsHolder.Get().Add(valueInt);
				_skillIndexHolder.Get().Add(valueInt2);
			}
			_stepHolder.Set(0);
		}
	}

	public override void OnUpdate(long currentGlobalTimeMs)
	{
		base.OnUpdate(currentGlobalTimeMs);
		int num = _stepHolder.Get();
		SnapshotList<int> snapshotList = _timeMsHolder.Get();
		SnapshotList<int> snapshotList2 = _skillIndexHolder.Get();
		if (num >= snapshotList.Count)
		{
			return;
		}
		long currentTimeMs = _battleInstance.MainTimer.GetCurrentTimeMs();
		List<int> list = null;
		while (num < snapshotList.Count && currentTimeMs >= snapshotList[num])
		{
			int num2 = snapshotList2[num];
			if (num2 != 0)
			{
				list = list ?? new List<int>();
				list.Add(num2);
			}
			num++;
			_stepHolder.Set(num);
		}
		if (list == null || list.Count <= 0)
		{
			return;
		}
		List<BattleCreature> allCreatures = _battleInstance.CreatureContainer.GetAllCreatures();
		for (int i = 0; i < list.Count; i++)
		{
			int key = list[i];
			SkillData data = GameRoot.Instance.SkillDataContainer.GetData(key);
			for (int j = 0; j < allCreatures.Count; j++)
			{
				allCreatures[j].SkillController.UseSkill(data, new SkillUseOption(checkOnly: false, checkLevel: false, checkMp: false, isEventSkill: true), null);
			}
		}
	}

	public int GetRemainNextStepTimeMs()
	{
		int num = _stepHolder.Get();
		SnapshotList<int> snapshotList = _timeMsHolder.Get();
		_skillIndexHolder.Get();
		if (num >= snapshotList.Count)
		{
			return -1;
		}
		long currentTimeMs = _battleInstance.MainTimer.GetCurrentTimeMs();
		return (int)(snapshotList[num] - currentTimeMs);
	}

	public override void OnEvent(IBattleTacticEvent tacticEvent, long currentGlobalTimeMs)
	{
	}
}
