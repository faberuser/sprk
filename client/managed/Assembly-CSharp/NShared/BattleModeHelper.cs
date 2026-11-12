using System;
using System.Collections.Generic;
using NShared.NUtil;

namespace NShared;

public static class BattleModeHelper
{
	private class WaveOffsetInfo
	{
		public int OffsetMxFromCenter = 5000;

		public int OffsetMxBetweenCreature = 1000;

		public int OffsetMzFromCenter = 1000;

		public int[] OffsetMz_Spread4 = new int[4] { -400, 3200, -2000, 1500 };
	}

	private static WaveOffsetInfo _offsetInfo;

	private static WaveOffsetInfo GetWaveOffsetInfo()
	{
		if (_offsetInfo != null)
		{
			return _offsetInfo;
		}
		_offsetInfo = new WaveOffsetInfo();
		if (GameRoot.Instance.ConstantDataContainer.GetData("BattleField_WaveReady_OffsetMxFromCenter") != null)
		{
			_offsetInfo.OffsetMxFromCenter = GameRoot.Instance.ConstantDataContainer.GetValueInt("BattleField_WaveReady_OffsetMxFromCenter");
		}
		if (GameRoot.Instance.ConstantDataContainer.GetData("BattleField_WaveReady_OffsetMxBetweenCreature") != null)
		{
			_offsetInfo.OffsetMxBetweenCreature = GameRoot.Instance.ConstantDataContainer.GetValueInt("BattleField_WaveReady_OffsetMxBetweenCreature");
		}
		if (GameRoot.Instance.ConstantDataContainer.GetData("BattleField_WaveReady_OffsetMzFromCenter") != null)
		{
			_offsetInfo.OffsetMzFromCenter = GameRoot.Instance.ConstantDataContainer.GetValueInt("BattleField_WaveReady_OffsetMzFromCenter");
		}
		return _offsetInfo;
	}

	public static List<Vector3> GetWaveReadyPosition(Vector3 center, int count, TeamType teamType, EBattleFormation formation = EBattleFormation.Default)
	{
		List<Vector3> list = new List<Vector3>();
		if (count <= 0)
		{
			return list;
		}
		WaveOffsetInfo waveOffsetInfo = GetWaveOffsetInfo();
		int num = ((teamType != TeamType.Left) ? 1 : (-1));
		if (formation == EBattleFormation.Default)
		{
			list.Add(center + new Vector3(waveOffsetInfo.OffsetMxFromCenter * num, 0L, 0L));
		}
		for (int i = list.Count; i < count; i++)
		{
			int num2 = waveOffsetInfo.OffsetMxFromCenter + i * waveOffsetInfo.OffsetMxBetweenCreature;
			int num3 = ((formation != EBattleFormation.Spread4 || i >= waveOffsetInfo.OffsetMz_Spread4.Length) ? ((i % 2 == 1) ? waveOffsetInfo.OffsetMzFromCenter : (-waveOffsetInfo.OffsetMzFromCenter)) : waveOffsetInfo.OffsetMz_Spread4[i]);
			list.Add(center + new Vector3(num2 * num, 0L, num3));
		}
		return list;
	}

	public static Vector3 GetWaveCenterPosition(int centerMz, int areaPositionMx, bool areaPositionIsRelative, BattleCreatureList leftCreatureList, BattleCreatureList rightCreatureList)
	{
		if (!areaPositionIsRelative)
		{
			return new Vector3(areaPositionMx, 0L, centerMz);
		}
		long minMx;
		long maxMx;
		bool creaturePositionMinMax = GetCreaturePositionMinMax(leftCreatureList, out minMx, out maxMx);
		long minMx2;
		long maxMx2;
		bool creaturePositionMinMax2 = GetCreaturePositionMinMax(rightCreatureList, out minMx2, out maxMx2);
		WaveOffsetInfo waveOffsetInfo = GetWaveOffsetInfo();
		if (!creaturePositionMinMax && !creaturePositionMinMax2)
		{
			return new Vector3(areaPositionMx, 0L, centerMz);
		}
		if (creaturePositionMinMax && !creaturePositionMinMax2)
		{
			return new Vector3(maxMx + waveOffsetInfo.OffsetMxFromCenter + areaPositionMx, 0L, centerMz);
		}
		if (!creaturePositionMinMax && creaturePositionMinMax2)
		{
			return new Vector3(minMx2 - waveOffsetInfo.OffsetMxFromCenter + areaPositionMx, 0L, centerMz);
		}
		return new Vector3((maxMx + minMx2) / 2 + areaPositionMx, 0L, centerMz);
	}

	private static bool GetCreaturePositionMinMax(BattleCreatureList leftCreatureList, out long minMx, out long maxMx)
	{
		if (leftCreatureList == null || leftCreatureList.Count == 0)
		{
			minMx = 0L;
			maxMx = 0L;
			return false;
		}
		minMx = leftCreatureList[0].TransformController.Position.Mx;
		maxMx = minMx;
		for (int i = 1; i < leftCreatureList.Count; i++)
		{
			if (leftCreatureList[i] != null)
			{
				int num = (int)leftCreatureList[i].TransformController.Position.Mx;
				minMx = Math.Min(minMx, num);
				maxMx = Math.Max(maxMx, num);
			}
		}
		return true;
	}
}
