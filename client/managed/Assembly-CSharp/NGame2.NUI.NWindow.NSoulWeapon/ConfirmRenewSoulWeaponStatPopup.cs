using System.Collections.Generic;
using NGame2.NUI.NComponent2.NSoulWeapon;
using NNguiExtension;
using NShared;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NWindow.NSoulWeapon;

public class ConfirmRenewSoulWeaponStatPopup : CommonRenewSoulWeaponStatPopup
{
	public interface IListener
	{
		void ConfirmRenewSoulWeaponStat(SoulWeaponInfo originSoulWeaponInfo, SoulWeaponInfo soulWeaponInfo, bool isRenew);
	}

	public static string PrefabPath = "Resources/Prefab2/Window/SoulWeapon/Popup/ConfirmRenewSoulWeaponStatPopup";

	public SoulWeaponStatInfo SoulWeaponStatInfo;

	public SoulWeaponTotalStatInfo SoulWeaponTotalStatInfo;

	public SoulWeaponStatInfo RenewSoulWeaponStatInfo;

	public SoulWeaponTotalStatInfo RenewSoulWeaponTotalStatInfo;

	public Transform SoulWeaponStatProgressRootTransform;

	public Transform SoulWeaponRenewStatProgressRootTransform;

	public List<UILabel> CompareStatLabelList = new List<UILabel>();

	public UISpriteSelector StatSpriteSelector;

	public UISpriteSelector RenewStatSpriteSelector;

	private IListener _iListener;

	private bool _isSelectRenew;

	private new SoulWeaponStatProgressComponent _statProgressComponent;

	private SoulWeaponStatProgressComponent _renewStatProgressComponent;

	public void Open(IListener iListener, SoulWeaponInfo soulWeaponInfo)
	{
		Open(soulWeaponInfo);
		Init(iListener);
	}

	public override void Close()
	{
		base.Close();
		_iListener = null;
		FactoryBase.SafeReleaseComponent(ref _statProgressComponent);
		FactoryBase.SafeReleaseComponent(ref _renewStatProgressComponent);
	}

	public override void OnSystemBackButton()
	{
	}

	private void Init(IListener iListener)
	{
		_iListener = iListener;
		SoulWeaponStatInfo.Open(base.SoulWeaponInfo);
		SoulWeaponTotalStatInfo.Open(base.SoulWeaponInfo, isRenew: false);
		RenewSoulWeaponStatInfo.Open(base.SoulWeaponInfo, isRenew: true);
		RenewSoulWeaponTotalStatInfo.Open(base.SoulWeaponInfo, isRenew: true);
		SetStatProgressComponent(base.SoulWeaponInfo, ref _statProgressComponent, ref SoulWeaponStatProgressRootTransform, isRenew: false);
		SetStatProgressComponent(base.SoulWeaponInfo, ref _renewStatProgressComponent, ref SoulWeaponRenewStatProgressRootTransform, isRenew: true);
		_statProgressComponent.EnableTouch(enable: false);
		_renewStatProgressComponent.EnableTouch(enable: false);
		SelectStat();
		SetCompareStat();
		PlaySound("UI.SoulWeapon.ResultStatusOption");
	}

	private void SetCompareStat()
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(CompareStatLabelList) || base.SoulWeaponInfo == null)
		{
			return;
		}
		long num = 0L;
		long num2 = 0L;
		long num3 = 0L;
		long num4 = 0L;
		UILabel uILabel = null;
		for (int i = 0; i < CompareStatLabelList.Count; i++)
		{
			uILabel = CompareStatLabelList[i];
			if (!(uILabel == null))
			{
				num = base.SoulWeaponInfo.GetBaseStat((ESoulWeaponStatType)i, isRenew: false);
				num2 = base.SoulWeaponInfo.GetBaseStat((ESoulWeaponStatType)i, isRenew: true);
				num3 = base.SoulWeaponInfo.GetBonusStat(num2, (ESoulWeaponStatType)i, isRenew: true) - base.SoulWeaponInfo.GetBonusStat(num, (ESoulWeaponStatType)i, isRenew: false);
				num4 = num2 - num + num3;
				UIHelper.SetLabelText(uILabel, (num4 > 0) ? GetCyanColorText("+" + num4) : num4.ToString());
			}
		}
	}

	private void SelectStat()
	{
		_isSelectRenew = false;
		if (StatSpriteSelector != null)
		{
			StatSpriteSelector.SelectByIndex(1);
		}
		if (RenewStatSpriteSelector != null)
		{
			RenewStatSpriteSelector.SelectByIndex(0);
		}
	}

	private void SelectRenewStat()
	{
		_isSelectRenew = true;
		if (StatSpriteSelector != null)
		{
			StatSpriteSelector.SelectByIndex(0);
		}
		if (RenewStatSpriteSelector != null)
		{
			RenewStatSpriteSelector.SelectByIndex(1);
		}
	}

	private string GetCyanColorText(string text)
	{
		return "[c][14f9f6]" + text + "[-][/c]";
	}

	public void OnClickStat()
	{
		SelectStat();
	}

	public void OnClickRenewStat()
	{
		SelectRenewStat();
	}

	public void OnClickConfirmRenewSoulWeaponStat()
	{
		if (RenewSoulWeaponStatInfo.SoulWeaponInfo != null && _iListener != null)
		{
			_iListener.ConfirmRenewSoulWeaponStat(base.SoulWeaponInfo, base.SoulWeaponInfo, _isSelectRenew);
		}
	}
}
