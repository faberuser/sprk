using System;
using System.Collections.Generic;
using NShared;

namespace NGame2.NUI.NWindow.NSoulWeapon;

[Serializable]
public class SoulWeaponStatValueInfo
{
	public UILabel ReinforceLevelLabel;

	public List<UILabel> StatNameLabelList = new List<UILabel>();

	public List<UILabel> StatLabelList = new List<UILabel>();

	public List<UILabel> BonusStatLabelList = new List<UILabel>();

	private SoulWeaponStatInfo _soulWeaponStatInfo;

	private List<StatType> _statTypeList = new List<StatType>();

	public void Open(SoulWeaponInfo soulWeaponInfo, bool isRenew = false, bool isTotal = false)
	{
		Init(soulWeaponInfo, isRenew, isTotal);
	}

	private void Init(SoulWeaponInfo soulWeaponInfo, bool isRenew, bool isTotal)
	{
		SetReinforceLevelLabel(soulWeaponInfo);
		SoulWeaponStatInfo soulWeaponStatInfo = new SoulWeaponStatInfo();
		if (soulWeaponStatInfo != null)
		{
			soulWeaponStatInfo.SetStatInfo(ref soulWeaponInfo, isRenew);
			soulWeaponStatInfo.SetEmptyLabel(ref StatNameLabelList, ref StatLabelList, ref BonusStatLabelList);
			soulWeaponStatInfo.SetStatTypeList(ref soulWeaponInfo, ref _statTypeList);
			soulWeaponStatInfo.SetStatName(ref _statTypeList, ref StatNameLabelList);
			soulWeaponStatInfo.SetStat(ref StatLabelList, ref BonusStatLabelList, isTotal, isRenew);
		}
	}

	private void SetReinforceLevelLabel(SoulWeaponInfo soulWeaponInfo)
	{
		UIHelper.SetLabelText(ReinforceLevelLabel, "-");
		if (soulWeaponInfo != null)
		{
			UIHelper.SetLabelText(ReinforceLevelLabel, soulWeaponInfo.ReinforceLevel);
		}
	}
}
