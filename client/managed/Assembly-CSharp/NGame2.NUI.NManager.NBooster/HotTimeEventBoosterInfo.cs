using System;
using NGame2.NUI.NWindow;
using NGame2.NUtil.NStringProxy;
using NVespa;

namespace NGame2.NUI.NManager.NBooster;

public class HotTimeEventBoosterInfo : BoosterInfo
{
	public HotTimeEventBoosterInfo()
	{
		base.Type = BoosterInfoType.BoosterHotTimeEvent;
	}

	public override int GetSortScore()
	{
		if (!String_Extension_Method.IsNullOrEmpty(base.ItemCode))
		{
			return 99999999;
		}
		return base.GetSortScore();
	}

	public override bool IsEffective()
	{
		DateTime uTCNow = NGame2.NUI.NWindow.Common.GetUTCNow();
		if (DateTime.Compare(uTCNow, base.EndTimeUTC) >= 0)
		{
			return false;
		}
		if (base.BeginHour != 0 || base.BeginMinute != 0 || base.EndHour != 0 || base.EndMinute != 0)
		{
			DateTime t = NVespa.Math.CreateDateTime(uTCNow.Year, uTCNow.Month, uTCNow.Day, base.BeginHour, base.BeginMinute, 0);
			DateTime t2 = NVespa.Math.CreateDateTime(uTCNow.Year, uTCNow.Month, uTCNow.Day, base.EndHour, base.EndMinute, 0);
			if (DateTime.Compare(uTCNow, t2) >= 0 || DateTime.Compare(uTCNow, t) <= 0)
			{
				return false;
			}
		}
		return true;
	}

	public override bool DisplayRemainTime()
	{
		return true;
	}

	public override int GetRemainSeconds()
	{
		if (base.BeginHour != 0 || base.BeginMinute != 0 || base.EndHour != 0 || base.EndMinute != 0)
		{
			DateTime uTCNow = NGame2.NUI.NWindow.Common.GetUTCNow();
			int num = (int)(NVespa.Math.CreateDateTime(uTCNow.Year, uTCNow.Month, uTCNow.Day, base.EndHour, base.EndMinute, 0) - uTCNow).TotalSeconds;
			int num2 = (int)(base.EndTimeUTC - uTCNow).TotalSeconds;
			if (num < num2)
			{
				return num;
			}
			return 0;
		}
		return base.GetRemainSeconds();
	}

	public override void SetupBoosterTooltip(TooltipTextInfo tooltipTextInfo)
	{
		tooltipTextInfo.Name.Value = base.Name;
		tooltipTextInfo.Desc.Value = base.Desc;
		if (base.BeginHour != 0 || base.BeginMinute != 0 || base.EndHour != 0 || base.EndMinute != 0)
		{
			tooltipTextInfo.TypeText.Assign(new TimeDurationBuilder(base.BeginHour, base.BeginMinute, base.EndHour, base.EndMinute, appendLine: false));
			return;
		}
		int remainSeconds = GetRemainSeconds();
		if (remainSeconds > 0)
		{
			tooltipTextInfo.TypeText.Assign(new BoosterTimeRemainedBuilder(remainSeconds, appendLine: false));
		}
		else
		{
			tooltipTextInfo.TypeText.Reset();
		}
	}
}
