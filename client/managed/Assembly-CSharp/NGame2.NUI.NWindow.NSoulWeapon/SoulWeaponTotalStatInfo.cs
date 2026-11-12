using System;
using System.Collections.Generic;
using NShared;

namespace NGame2.NUI.NWindow.NSoulWeapon;

[Serializable]
public class SoulWeaponTotalStatInfo
{
	public List<UILabel> StatLabelList = new List<UILabel>();

	private SoulWeaponInfo _soulWeaponInfo;

	public void Open(SoulWeaponInfo soulWeaponInfo, bool isRenew, EStatType eStatType = EStatType.Default)
	{
		Init(soulWeaponInfo, isRenew, eStatType);
	}

	private void Init(SoulWeaponInfo soulWeaponInfo, bool isRenew, EStatType eStatType)
	{
		_soulWeaponInfo = soulWeaponInfo;
		SetStatLabelList(isRenew, eStatType);
	}

	private void SetStatLabelList(bool isRenew, EStatType eStatType)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(StatLabelList) || _soulWeaponInfo == null)
		{
			return;
		}
		long num = 0L;
		long num2 = 0L;
		UILabel uILabel = null;
		for (int i = 0; i < StatLabelList.Count; i++)
		{
			uILabel = StatLabelList[i];
			if (!(uILabel == null))
			{
				switch (eStatType)
				{
				case EStatType.Max:
					num = _soulWeaponInfo.GetMaxStat((ESoulWeaponStatType)i);
					num2 = _soulWeaponInfo.GetMaxBonusStat(num, (ESoulWeaponStatType)i);
					break;
				case EStatType.Min:
					num = _soulWeaponInfo.GetMinStat((ESoulWeaponStatType)i);
					num2 = _soulWeaponInfo.GetMaxBonusStat(num, (ESoulWeaponStatType)i);
					break;
				default:
					num = _soulWeaponInfo.GetBaseStat((ESoulWeaponStatType)i, isRenew);
					num2 = _soulWeaponInfo.GetBonusStat(num, (ESoulWeaponStatType)i, isRenew);
					break;
				}
				UIHelper.SetLabelText(uILabel, GetColorTotalStat((num + num2).ToString()) + " (" + num + "  + " + GetColorBonusStat(num2.ToString()) + ")");
			}
		}
	}

	private string GetColorTotalStat(string text)
	{
		return "[c][019a9a]" + text + "[-][/c]";
	}

	private string GetColorBaseStat(string text)
	{
		return text;
	}

	private string GetColorBonusStat(string text)
	{
		return "[c][00be00]" + text + "[-][/c]";
	}
}
