using System;
using System.Collections.Generic;

namespace NShared.NOperationGroupCondition;

public class Random : IOperationGroupCondition
{
	void IOperationGroupCondition.Check(IList<ModifiedFiredOperationInfo> modifiedInfos)
	{
		if (modifiedInfos == null)
		{
			return;
		}
		int count = modifiedInfos.Count;
		if (count == 0)
		{
			return;
		}
		ArrayPool<int>.AutoArrayPoolReleaser ratios = ArrayPool<int>.AllocAuto(count);
		try
		{
			int sumRatio = 0;
			int index;
			for (index = 0; index < count; index++)
			{
				modifiedInfos[index]?.GetConditionValues(delegate(string[] values)
				{
					int valueInt = ArgParser.GetValueInt(values, 0);
					sumRatio += valueInt;
					ratios[index] = valueInt;
				});
			}
			int num = 0;
			BattleCreature firedCreature = modifiedInfos[0].FiredOperationInfo.FiredCreature;
			if (sumRatio == 0)
			{
				num = firedCreature.NextRandom(count, "NOperationGroupCondition.Random");
			}
			else
			{
				int num2 = firedCreature.NextRandom(sumRatio, "NOperationGroupCondition.Random");
				sumRatio = 0;
				for (int num3 = 0; num3 < count; num3++)
				{
					sumRatio += ratios[num3];
					if (sumRatio > num2)
					{
						num = num3;
						break;
					}
				}
			}
			for (int num4 = 0; num4 < modifiedInfos.Count; num4++)
			{
				modifiedInfos[num4].IsValid = num4 == num;
			}
		}
		finally
		{
			if (ratios != null)
			{
				((IDisposable)ratios).Dispose();
			}
		}
	}
}
