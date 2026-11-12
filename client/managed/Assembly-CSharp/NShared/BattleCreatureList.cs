using System;
using System.Collections.Generic;
using System.Linq;
using NShared.NUtil;
using NVespa;

namespace NShared;

public class BattleCreatureList : ReserveList<BattleCreature>, ISnapshotObject
{
	private long _uid;

	public SnapshotRefCounter RefCounter { get; private set; }

	public ISnapshotObjectPool Pool { get; set; }

	public bool PendingRelease { get; set; }

	public static BattleCreatureList Alloc()
	{
		return SnapshotObjectPool<BattleCreatureList>.Alloc();
	}

	public static BattleCreatureList AllocWithCreature(BattleCreature creature)
	{
		BattleCreatureList battleCreatureList = SnapshotObjectPool<BattleCreatureList>.Alloc();
		if (creature != null)
		{
			battleCreatureList.Add(creature);
		}
		return battleCreatureList;
	}

	object ISnapshotObject.Serialize(SerializedSnapshotContainer container)
	{
		VespaDebug.LogError("Invalid ISnapshotObject.Serialize");
		return null;
	}

	void ISnapshotObject.Deserialize(object s)
	{
		VespaDebug.LogError("Invalid ISnapshotObject.Deserialize");
	}

	string ISnapshotObject.Dump()
	{
		VespaDebug.LogError("Invalid ISnapshotObject.Dump");
		return string.Empty;
	}

	void ISnapshotObject.Parse(IList<string> dumped, DumpedSnapshotContainer container)
	{
		VespaDebug.LogError("Invalid ISnapshotObject.Parse");
	}

	long ISnapshotObject.GetUid()
	{
		return _uid;
	}

	void ISnapshotObject.SetUid(long uid)
	{
		_uid = uid;
	}

	string ISnapshotObject.GetTypeName()
	{
		return "BattleCreatureList";
	}

	void ISnapshotObject.OnRelease()
	{
		RemoveRange(0, base.Count);
	}

	IList<ISnapshotObject> ISnapshotObject.GetChildren()
	{
		VespaDebug.LogError("Invalid ISnapshotObject.GetChildren");
		return null;
	}

	public BattleCreatureList()
	{
		RefCounter = new SnapshotRefCounter();
		_uid = SnapshotUidAllocator.Allocate();
	}

	private void Filter(Func<BattleCreature, bool> condition)
	{
		for (int i = 0; i < base.Count; i++)
		{
			BattleCreature battleCreature = base[i];
			if (battleCreature == null || !condition(battleCreature))
			{
				RemoveAt(i);
				i--;
			}
		}
	}

	public void FilterByDefault(BattleCreature firedCreature, SkillTargetType skillTargetType, string[] stateTypes, bool deadTarget, bool ignoreBlock, SkillTargetFlag skillTargetFlag)
	{
		bool flag = false;
		long leftMaxMx = long.MinValue;
		long rightMinMx = long.MaxValue;
		Vector3 position = firedCreature.TransformController.Position;
		for (int i = 0; i < base.Count; i++)
		{
			BattleCreature battleCreature = base[i];
			if (battleCreature == null || battleCreature.StatController.IsDead() != deadTarget || !battleCreature.StateController.CanBeTargetedState(firedCreature) || !battleCreature.StateController.IsInAnyState(stateTypes) || (battleCreature.CreateInfo.IsBoss && (skillTargetFlag & SkillTargetFlag.ExcludeBoss) != SkillTargetFlag.None))
			{
				RemoveAt(i);
				i--;
				continue;
			}
			if (battleCreature.BattleCreatureData.IsAvoidSkillTargetType(skillTargetType.GetString()))
			{
				RemoveAt(i);
				i--;
				continue;
			}
			bool num = (battleCreature.CreatureData.TransformType & CreatureTransformType.Wall) != 0;
			bool flag2 = battleCreature.CreatureData.BlockDash && !ignoreBlock;
			if (num || flag2)
			{
				Vector3 position2 = battleCreature.TransformController.Position;
				if (position2.Mx >= position.Mx)
				{
					rightMinMx = System.Math.Min(rightMinMx, position2.Mx);
				}
				else
				{
					leftMaxMx = System.Math.Min(leftMaxMx, position2.Mx);
				}
				flag = true;
			}
		}
		if (flag)
		{
			Filter((BattleCreature e) => e.TransformController.Position.Mx >= leftMaxMx && e.TransformController.Position.Mx <= rightMinMx);
		}
	}

	public override void Sort(Comparison<BattleCreature> comparison)
	{
		base.Sort(delegate(BattleCreature l, BattleCreature r)
		{
			int num = comparison(l, r);
			return (num != 0) ? num : l.Uid.CompareTo(r.Uid);
		});
	}

	public void SortByMaxHpRatio()
	{
		Sort(delegate(BattleCreature l, BattleCreature r)
		{
			int hpRatioM = l.StatController.HpRatioM;
			return r.StatController.HpRatioM.CompareTo(hpRatioM);
		});
	}

	public void SortByMinHpRatio()
	{
		Sort(delegate(BattleCreature l, BattleCreature r)
		{
			int hpRatioM = l.StatController.HpRatioM;
			int hpRatioM2 = r.StatController.HpRatioM;
			return hpRatioM.CompareTo(hpRatioM2);
		});
	}

	public void SortByMaxStat(StatType statType)
	{
		Sort(delegate(BattleCreature l, BattleCreature r)
		{
			long finalStat = l.StatController.GetFinalStat(statType);
			return r.StatController.GetFinalStat(statType).CompareTo(finalStat);
		});
	}

	public void SortByMinStat(StatType statType)
	{
		Sort(delegate(BattleCreature l, BattleCreature r)
		{
			long finalStat = l.StatController.GetFinalStat(statType);
			long finalStat2 = r.StatController.GetFinalStat(statType);
			return finalStat.CompareTo(finalStat2);
		});
	}

	public void SortByDistance(BattleCreature pivotCreature, bool near, SkillData skillData = null)
	{
		Vector3 firedPosition = pivotCreature.TransformController.Position;
		Sort((BattleCreature l, BattleCreature r) => CompareCreatureByDistance(l, r, firedPosition, near));
	}

	public void SortByTotalDamage(BattleStatisticsManager statisticsManager, bool highest)
	{
		Sort(delegate(BattleCreature l, BattleCreature r)
		{
			long value = ((statisticsManager != null) ? statisticsManager.TotalDamageMeter(l.Uid) : l.Uid);
			long value2 = ((statisticsManager != null) ? statisticsManager.TotalDamageMeter(r.Uid) : r.Uid);
			return highest ? value2.CompareTo(value) : value.CompareTo(value2);
		});
	}

	public void SortByThreatPoint(bool highest)
	{
		Sort(delegate(BattleCreature l, BattleCreature r)
		{
			long allRecordSum = l.ThreatHistoryTracker.GetAllRecordSum();
			long allRecordSum2 = r.ThreatHistoryTracker.GetAllRecordSum();
			return highest ? allRecordSum2.CompareTo(allRecordSum) : allRecordSum.CompareTo(allRecordSum2);
		});
	}

	public void SortBySkillDistance(BattleCreature pivotCreature, int minRangeMm, int maxRangeMm, bool near)
	{
		Vector3 firedPosition = pivotCreature.TransformController.Position;
		Sort(delegate(BattleCreature l, BattleCreature r)
		{
			bool num = BattleCreatureContainer.IsInSkillRange(pivotCreature, l, minRangeMm, maxRangeMm) == SkillRangeResultType.Success;
			bool flag = BattleCreatureContainer.IsInSkillRange(pivotCreature, r, minRangeMm, maxRangeMm) == SkillRangeResultType.Success;
			if (num && flag)
			{
				long wavePositionScore = l.CreateInfo.GetWavePositionScore();
				return r.CreateInfo.GetWavePositionScore().CompareTo(wavePositionScore);
			}
			return CompareCreatureByDistance(l, r, firedPosition, near);
		});
	}

	public void FilterBySkillDistance(BattleCreature pivotCreature, int minRangeMm, int maxRangeMm)
	{
		Filter((BattleCreature creature) => BattleCreatureContainer.IsInSkillRange(pivotCreature, creature, minRangeMm, maxRangeMm) == SkillRangeResultType.Success);
	}

	private static int CompareCreatureByDistance(BattleCreature l, BattleCreature r, Vector3 firedPosition, bool near)
	{
		int aggroRate = BattleConstant.AggroRate;
		long num = Vector3.DistanceWithoutY(firedPosition, l.TransformController.Position);
		long num2 = Vector3.DistanceWithoutY(firedPosition, r.TransformController.Position);
		long num3 = num - l.CreatureData.SizeMm;
		long num4 = num2 - r.CreatureData.SizeMm;
		if (near)
		{
			num3 += l.TransformController.GetRangeBonusMm() * aggroRate / 1000;
			num4 += r.TransformController.GetRangeBonusMm() * aggroRate / 1000;
			return num3.CompareTo(num4);
		}
		return num4.CompareTo(num3);
	}

	public void Shuffle()
	{
		if (base.Count != 0)
		{
			BattleCreature battleCreature = base[0];
			for (int num = base.Count - 1; num > 0; num--)
			{
				int index = battleCreature.NextRandom(num + 1, "BattleCreatureList.Shuffle");
				BattleCreature value = base[index];
				base[index] = base[num];
				base[num] = value;
			}
		}
	}

	public void RemainIfNotZero(int remainedCount)
	{
		if (remainedCount != 0 && base.Count > remainedCount)
		{
			RemoveRange(remainedCount, base.Count - remainedCount);
		}
	}

	public void RemoveTarget(BattleCreature[] removeTargets)
	{
		if (removeTargets != null && removeTargets.Length != 0)
		{
			Filter((BattleCreature creature) => removeTargets.Contains(creature));
		}
	}
}
