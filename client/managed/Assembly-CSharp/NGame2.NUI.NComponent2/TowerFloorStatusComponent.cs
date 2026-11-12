using System;
using NGame2.NUI.NHelper;
using NShared;
using NVespa.NGlobal;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class TowerFloorStatusComponent : LinkFactoryComponent
{
	[Serializable]
	public class TowerFloorStatus
	{
		public UILabel SweepFloorLabel;

		public UILabel SweepCountLabel;
	}

	public const string PrefabPath = "Prefab2/Component/MessagePopup/TowerFloorStatusComponent";

	public static PrefabAuto<TowerFloorStatusComponent> PrefabAuto = new PrefabAuto<TowerFloorStatusComponent>("Prefab2/Component/MessagePopup/TowerFloorStatusComponent");

	public TowerFloorStatus SequnceSweep;

	public TowerFloorStatus RepeatSweep;

	private SweepDungeonType _sweepDungeonType;

	private int _start;

	private long _end;

	private int _topStep;

	private long _sweepCount;

	public void Open(SweepDungeonType sweepDungeonType, int startFloor, long endFloor, int topStep, long sweepCount)
	{
		_sweepDungeonType = sweepDungeonType;
		_start = startFloor;
		_end = endFloor;
		_topStep = topStep;
		_sweepCount = sweepCount;
		Init();
	}

	private void Init()
	{
		int num = 0;
		long num2 = num;
		int num3 = 0;
		long num4 = num3;
		int start = _start;
		long end = _end;
		int num5 = start;
		if (num5 == end)
		{
			long end2 = _end;
			int topStep = _topStep;
			if (end2 == topStep)
			{
				num4 = _sweepCount;
				goto IL_00c7;
			}
		}
		long end3 = _end;
		int start2 = _start;
		checked
		{
			long num6 = end3 - start2;
			long num7;
			if (num6 >= _sweepCount)
			{
				num7 = num6;
			}
			else
			{
				int num8 = 1;
				num7 = num6 + num8;
			}
			num2 = num7;
			num4 = _sweepCount - num2;
			long num9 = num4;
			int num10 = 0;
			long num11;
			if (num9 >= num10)
			{
				num11 = num4;
			}
			else
			{
				int num12 = 0;
				num11 = num12;
			}
			num4 = num11;
			goto IL_00c7;
		}
		IL_00c7:
		SetSequnceSweep(num2);
		SetRepeatSweep(num4);
	}

	private void SetSequnceSweep(long count)
	{
		if (SequnceSweep == null)
		{
			return;
		}
		SweepDungeonType sweepDungeonType = _sweepDungeonType;
		int start = _start;
		string stepLocalText = SweepHelper.GetStepLocalText(sweepDungeonType, start);
		string stepLocalText2 = SweepHelper.GetStepLocalText(_sweepDungeonType, _end);
		string text = "-";
		string text2 = "-";
		int num = 1;
		if (count == num)
		{
			text = $"{stepLocalText}";
			text2 = LocalizationManager.GetStringDirect("SWEEP_NOTICE_INFO_CONTINUOUS_COUNT_DESC", count);
		}
		else
		{
			int num2 = 1;
			if (count > num2)
			{
				text = $"{stepLocalText} ~ {stepLocalText2}";
				text2 = LocalizationManager.GetStringDirect("SWEEP_NOTICE_INFO_CONTINUOUS_COUNT_DESC", count);
			}
		}
		UIHelper.SetLabelText(SequnceSweep.SweepFloorLabel, text);
		UIHelper.SetLabelText(SequnceSweep.SweepCountLabel, text2);
	}

	private void SetRepeatSweep(long count)
	{
		if (RepeatSweep != null)
		{
			string stepLocalText = SweepHelper.GetStepLocalText(_sweepDungeonType, _end);
			string text = "-";
			string text2 = "-";
			if (count != 0L)
			{
				text = stepLocalText;
				text2 = LocalizationManager.GetStringDirect("SWEEP_NOTICE_INFO_CONTINUOUS_COUNT_DESC", count);
			}
			UIHelper.SetLabelText(RepeatSweep.SweepFloorLabel, text);
			UIHelper.SetLabelText(RepeatSweep.SweepCountLabel, text2);
		}
	}
}
